using TextToSpeech.Core.Config;
using TextToSpeech.Core.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using static TextToSpeech.Core.Enums;
using System.Text;
using Microsoft.AspNetCore.Http;
using TextToSpeech.Core.Interfaces.Repositories;
using TextToSpeech.Core.Interfaces;
using TextToSpeech.Infra.Services.FileProcessing;
using TextToSpeech.Core.Interfaces.Ai;
using TextToSpeech.Infra.SignalR;
using System.Collections.Concurrent;
using TextToSpeech.Core;
using Microsoft.Extensions.DependencyInjection;
using TextToSpeech.Infra.Interfaces;

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
    public async Task<Guid> CreateSpeech(Core.Dto.SpeechRequest request) // todo rename GetOrInitiateSpeech
    {
        ArgumentNullException.ThrowIfNull(request.File);

        var fileText = await ExtractText(request.File);

        var hash = AudioFileBuilder.GenerateAudioFileHash(Encoding.UTF8.GetBytes(fileText),
            request.Voice, request.LanguageCode, request.Speed);

        var audioFileId = await _redisCacheProvider.GetCachedData<Guid?>(hash)
            ?? (await _audioFileRepository.GetAudioFileByHashAsync(hash))?.Id;

        if (audioFileId is not null)
        {
            _ = UpdateAudioStatus(audioFileId.Value, Status.Completed.ToString(), delayMs: 100);
            return audioFileId.Value;
        }

        audioFileId = Guid.NewGuid();

        var cts = new CancellationTokenSource();
        _taskManager.AddTask(audioFileId.Value, cts);

        _backgroundTaskQueue.QueueBackgroundWorkItem(async token =>
        {
            token = cts.Token;
            using var scope = _serviceScopeFactory.CreateScope();
            var speechService = scope.ServiceProvider.GetRequiredService<ISpeechService>();
            await speechService.ProcessSpeechAsync(request, audioFileId.Value, fileText, hash, token);
        });

        return audioFileId.Value;
    }

    public async Task ProcessSpeechAsync(Core.Dto.SpeechRequest request,
        Guid fileId,
        string fileText,
        string hash,
        CancellationToken cancellationToken)
    {
        AudioFile audioFile = null!;
        string? errorMessage = null;
        try
        {
            _logger.LogInformation($"Processing speech for {fileId}");

            audioFile = AudioFileBuilder.Create([],
            request.Voice,
            request.LanguageCode,
            request.Speed,
            AudioType.Full,
            SharedConstants.TtsApis.Single(kv => kv.Key == request.TtsApi).Value,
            request.File!.FileName,
            fileId);

            var ttsService = _ttsServiceFactory.Get(request.TtsApi);

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
                request.Voice,
                fileId,
                request.Speed,
                progress,
                cancellationToken);

            var bytes = AudioFileService.ConcatenateMp3Files(bytesCollection);

            var localFilePath = _pathService.GetFilePathInFileStorage($"{audioFile.Id}.mp3");

            await File.WriteAllBytesAsync(localFilePath, bytes, cancellationToken);

            audioFile.Status = Status.Completed;
            audioFile.Data = bytes;
            audioFile.Hash = hash;

            _metaDataService.AddMetaData(localFilePath, request.File!.FileName);

            await _redisCacheProvider.SetCachedData(audioFile.Hash, audioFile.Id, TimeSpan.FromDays(365));
            await _audioFileRepository.AddAudioFileAsync(audioFile);
        }
        catch (OperationCanceledException)
        {
            audioFile.Status = Status.Canceled;
            _logger.LogInformation($"Speech processing was canceled for {fileId}");
        }
        catch (Exception ex)
        {
            audioFile.Status = Status.Failed;
            errorMessage = ex.Message;
            _logger.LogError(ex, $"Error on processing speech {fileId}");
             //todo track status while processing, throw as needed
            throw;
        }
        finally
        {
            await UpdateAudioStatus(audioFile.Id, audioFile.Status.ToString(), errorMessage: errorMessage);
        }
    }

    public async Task<MemoryStream> CreateSpeechSample(Core.Dto.SpeechRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Input))
        {
            throw new ArgumentException(nameof(request.Input));
        }

        var hash = AudioFileBuilder.GenerateAudioFileHash(Encoding.UTF8.GetBytes(request.Input),
            request.Voice, request.LanguageCode, request.Speed);

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

        var bytesCollection = await _ttsServiceFactory.Get(request.TtsApi)
            .RequestSpeechSample(request.Input, request.Voice, request.Speed);

        audioFile = AudioFileBuilder.Create(bytesCollection.ToArray(),
            request.Voice,
            request.LanguageCode,
            request.Speed,
            AudioType.Sample,
            SharedConstants.TtsApis.Single(kv => kv.Key == request.TtsApi).Value);

        audioFile.Status = Status.Completed;

        await _redisCacheProvider.SetCachedData(hash, audioFile, TimeSpan.FromDays(365));
        await _audioFileRepository.AddAudioFileAsync(audioFile);

        return new MemoryStream(audioFile.Data);
    }

    private async Task<string> ExtractText(IFormFile file)
    {
        var fileProcessor = _fileProcessorFactory.GetProcessor(Path.GetExtension(file.FileName)) ??
            throw new NotSupportedException("File type not supported");

        var fileText = await fileProcessor.ExtractTextAsync(file);
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
    // todo Ensure that your repository services (_audioFileRepositoryService) handle concurrency and transaction management effectively, especially in the UpdateAudioFileAsync method.

    private bool ShouldTriggerUpdate(Guid fileId, int progress)
    {
        if (!_lastProgressDictionary.TryGetValue(fileId, out var lastProgress))
        {
            return true;
        }

        return progress > lastProgress;
    }
}
