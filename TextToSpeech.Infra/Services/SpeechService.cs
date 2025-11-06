using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using TextToSpeech.Core;
using TextToSpeech.Core.Config;
using TextToSpeech.Core.Dto;
using TextToSpeech.Core.Entities;
using TextToSpeech.Core.Interfaces;
using TextToSpeech.Core.Interfaces.Ai;
using TextToSpeech.Core.Interfaces.Repositories;
using TextToSpeech.Infra.Interfaces;
using TextToSpeech.Infra.Services.FileProcessing;
using TextToSpeech.Infra.SignalR;
using static TextToSpeech.Core.Enums;

namespace TextToSpeech.Infra.Services;

public sealed class SpeechService(ITextProcessingService _textFileService,
    ITtsServiceFactory _ttsServiceFactory,
    IPathService _pathService,
    IFileProcessorFactory _fileProcessorFactory,
    IHubContext<AudioHub> _hubContext,
    ILogger<SpeechService> _logger,
    IMetaDataService _metaDataService,
    IAudioFileRepository _audioFileRepository,
    ITaskManager _taskManager,
    IBackgroundTaskQueue _backgroundTaskQueue,
    IServiceScopeFactory _serviceScopeFactory,
    IRedisCacheProvider _redisCacheProvider) : ISpeechService
{
    private readonly ConcurrentDictionary<Guid, int> _lastProgressDictionary = new();

    /// <summary>
    /// Process the speech asynchronously without waiting for it to complete to return the ID immediately to the client
    /// </summary>
    /// <param name="request"></param>
    /// <returns>ID of the newly created audio file</returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<Guid> GetOrInitiateSpeech(TtsRequestOptions request, byte[] fileBytes, string fileName,
        string langCode, string ttsApi)
    {
        var fileText = await ExtractText(fileBytes, fileName);

        var hash = AudioFileBuilder.GenerateHash(fileText, langCode, request);

        var audioFileId = await _redisCacheProvider.GetCachedData<Guid?>(hash)
            ?? (await _audioFileRepository.GetAudioFileByHashAsync(hash))?.Id;

        if (audioFileId is not null)
        {
            _ = UpdateAudioStatus(audioFileId.Value, Status.Completed.ToString(), delayMs: 100);
            _logger.LogInformation("Audio is found for {audioFileId}", audioFileId);
            return audioFileId.Value;
        }

        audioFileId = Guid.NewGuid();

        var cts = new CancellationTokenSource();
        _taskManager.AddTask(audioFileId.Value, cts);

        _ = UpdateAudioStatus(audioFileId.Value, Status.Created.ToString(), delayMs: 100);

        _backgroundTaskQueue.QueueBackgroundWorkItem(async token =>
        {
            token = cts.Token;
            using var scope = _serviceScopeFactory.CreateScope();
            var speechService = scope.ServiceProvider.GetRequiredService<ISpeechService>();
            await speechService.ProcessSpeechAsync(request, fileText, fileName, langCode, ttsApi,
                audioFileId.Value, hash, token);
        });

        _logger.LogInformation("Initializing TTS for {audioFileId}", audioFileId);

        return audioFileId.Value;
    }

    public async Task ProcessSpeechAsync(TtsRequestOptions request,
        string fileText,
        string fileName,
        string langCode,
        string ttsApi,
        Guid fileId,
        string hash,
        CancellationToken cancellationToken)
    {
        AudioFile audioFile = null!;
        string? errorMessage = null;
        try
        {
            _logger.LogInformation("Processing speech for {fileId}", fileId);

            audioFile = AudioFileBuilder.Create([],
                langCode,
                AudioType.Full,
                fileText,
                request,
                SharedConstants.TtsApis.Single(kv => kv.Key.Equals(ttsApi, StringComparison.OrdinalIgnoreCase)).Value,
                fileName,
                fileId
            );

            audioFile.Status = Status.Processing;

            _ = UpdateAudioStatus(audioFile.Id, audioFile.Status.ToString());

            var ttsService = _ttsServiceFactory.Get(ttsApi);

            var textChunks = _textFileService.SplitTextIfGreaterThan(fileText, ttsService.MaxLengthPerApiRequest);

            var progress = new Progress<ProgressReport>();
            progress.ProgressChanged += async (sender, report) =>
            {
                if (ShouldTriggerUpdate(report.FileId, report.ProgressPercentage))
                {
                    await UpdateAudioStatus(report.FileId, Status.Processing.ToString(), report.ProgressPercentage).ConfigureAwait(false);
                    _lastProgressDictionary[fileId] = report.ProgressPercentage;
                }
            };

            var bytesCollection = await ttsService.RequestSpeechChunksAsync(textChunks,
                fileId,
                request,
                progress,
                cancellationToken);

            var bytes = AudioFileService.ConcatenateMp3Files(bytesCollection);

            var localFilePath = _pathService.ResolveFilePathForStorage(audioFile.Id,
                request.ResponseFormat.ToString());

            await File.WriteAllBytesAsync(localFilePath, bytes, cancellationToken);

            audioFile.Status = Status.Completed;
            audioFile.Data = bytes;

            _metaDataService.AddMetaData(localFilePath, fileName);

            await _redisCacheProvider.SetCachedData(audioFile.Hash, audioFile.Id, TimeSpan.FromDays(365));
            await _audioFileRepository.AddAudioFileAsync(audioFile);
        }
        catch (OperationCanceledException)
        {
            audioFile.Status = Status.Canceled;
        }
        catch (Exception ex)
        {
            audioFile.Status = Status.Failed;
            errorMessage = ex.Message;
            throw;
        }
        finally
        {
            await UpdateAudioStatus(audioFile.Id, audioFile.Status.ToString(), errorMessage: errorMessage);
            _logger.LogInformation("Speech processing is {status} for {fileId}", audioFile.Status, fileId);
        }
    }

    public async Task<MemoryStream> CreateSpeechSample(TtsRequestOptions request, string input, string langCode, string ttsApi)
    {
        var hash = AudioFileBuilder.GenerateHash(input, langCode, request);

        var audioFile = await _redisCacheProvider.GetCachedData<AudioFile>(hash);

        if (audioFile is null)
        {
            audioFile = await _audioFileRepository.GetAudioFileByHashAsync(hash);

            if (audioFile is not null)
            {
                await _redisCacheProvider.SetCachedData(hash, audioFile, TimeSpan.FromDays(365));
            }
        }

        if (audioFile is not null)
        {
            return new MemoryStream(audioFile.Data);
        }

        var bytesCollection = await _ttsServiceFactory.Get(ttsApi)
            .RequestSpeechSample(input, request);

        audioFile = AudioFileBuilder.Create(bytesCollection.ToArray(),
            langCode,
            AudioType.Sample,
            input,
            request,
            SharedConstants.TtsApis.Single(kv => kv.Key.Equals(ttsApi, StringComparison.OrdinalIgnoreCase)).Value,
            hash: hash);

        audioFile.Status = Status.Completed;

        await _redisCacheProvider.SetCachedData(hash, audioFile, TimeSpan.FromDays(365));
        await _audioFileRepository.AddAudioFileAsync(audioFile);

        return new MemoryStream(audioFile.Data);
    }

    private async Task<string> ExtractText(byte[] fileBytes, string fileName)
    {
        var fileProcessor = _fileProcessorFactory.GetProcessor(Path.GetExtension(fileName)) ??
            throw new NotSupportedException("File type not supported");

        var fileText = await fileProcessor.ExtractTextAsync(fileBytes);
        return fileText;
    }

    /// <summary>
    /// Notify clients about the status update
    /// </summary>
    private async Task UpdateAudioStatus(Guid audioFileId,
        string status,
        int? progressPercentage = null,
        string? errorMessage = null,
        int? delayMs = null)
    {
        if (delayMs is not null)
        {
            await Task.Delay(delayMs.Value);
        }

        await _hubContext.Clients.All.SendAsync(SharedConstants.AudioStatusUpdated, audioFileId.ToString(), status, progressPercentage, errorMessage);
    }

    private bool ShouldTriggerUpdate(Guid fileId, int progress)
    {
        if (!_lastProgressDictionary.TryGetValue(fileId, out var lastProgress))
        {
            return true;
        }

        return progress > lastProgress;
    }
}
