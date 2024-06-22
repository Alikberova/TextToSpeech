using TextToSpeech.Core.Config;
using TextToSpeech.Core.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using static TextToSpeech.Core.Enums;
using System.Text;
using Microsoft.AspNetCore.Http;
using TextToSpeech.Core.Interfaces.Repositories;
using TextToSpeech.Core.Interfaces;
using TextToSpeech.Infra.Services.FileProcessing;
using TextToSpeech.Core.Interfaces.Ai;

namespace TextToSpeech.Infra.Services;

public sealed class SpeechService(ITextProcessingService textFileService,
    ITtsServiceFactory ttsServiceFactory,
    IPathService pathService,
    IFileProcessorFactory fileProcessorFactory,
    IFileStorageService fileStorageService,
    IHubContext<AudioHub> hubContext,
    ILogger<SpeechService> logger,
    IMetaDataService metaDataService,
    IAudioFileRepository audioFileRepository,
    IRedisCacheProvider redisCacheProvider) : ISpeechService
{
    private readonly ITextProcessingService _textFileService = textFileService;
    private readonly ITtsServiceFactory _ttsServiceFactory = ttsServiceFactory;
    private readonly IPathService _pathService = pathService;
    private readonly IFileProcessorFactory _fileProcessorFactory = fileProcessorFactory;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly IHubContext<AudioHub> _hubContext = hubContext;
    private readonly ILogger<SpeechService> _logger = logger;
    private readonly IMetaDataService _metaDataService = metaDataService;
    private readonly IAudioFileRepository _audioFileRepository = audioFileRepository;
    private readonly IRedisCacheProvider _redisCacheProvider = redisCacheProvider;

    /// <summary>
    /// Process the speech asynchronously without waiting for it to complete to return the ID immediately to the client
    /// </summary>
    /// <param name="request"></param>
    /// <returns>ID of the newly created audio file</returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<Guid> CreateSpeech(Core.Dto.SpeechRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.File);

        var fileText = await ExtractContent(request.File);

        var hash = AudioFileBuilder.GenerateAudioFileHash(Encoding.UTF8.GetBytes(fileText),
            request.Voice, request.LanguageCode, request.Speed);

        var audioFileId = await _redisCacheProvider.GetCachedData<Guid>(hash);

        if (audioFileId != Guid.Empty)
        {
            _ = UpdateAudioStatus(audioFileId, Status.Completed.ToString(), delayMs: 100);
            return audioFileId;
        }

        var dbAudioFile = await _audioFileRepository.GetAudioFileByHashAsync(hash);

        if (dbAudioFile is not null)
        {
            _ = UpdateAudioStatus(dbAudioFile.Id, Status.Completed.ToString(), delayMs: 100);
            return dbAudioFile.Id;
        }

        var audioFile = new AudioFile
        {
            Id = Guid.NewGuid(),
            Status = Status.Processing,
            CreatedAt = DateTime.UtcNow,
            Hash = hash,
        };

        var fileId = audioFile.Id;

        _ = ProcessSpeechAsync(request, audioFile);

        return fileId;
    }

    internal async Task ProcessSpeechAsync(Core.Dto.SpeechRequest request, AudioFile audioFile)
    {
        try
        {
            // todo handle errors
            string fileText = await ExtractContent(request.File!);

            var ttsService = _ttsServiceFactory.Get(request.TtsApi);

            var textChunks = _textFileService.SplitTextIfGreaterThan(fileText, ttsService.MaxInputLength); //todo rename to MaxLengthPerApiRequest

            var bytesCollection = await ttsService.RequestSpeechChunksAsync(textChunks, request.Voice, request.Speed, request.Model);

            var bytes = AudioFileService.ConcatenateMp3Files(bytesCollection);

            var localFilePath = _pathService.GetFileStorageFilePath($"{audioFile.Id}.mp3");

            await File.WriteAllBytesAsync(localFilePath, bytes);

            audioFile.Status = Status.Completed;
            audioFile.Data = bytes;

            try
            {
                _metaDataService.AddMetaData(localFilePath, request.File!.FileName);
            }
            catch (Exception ex)
            {
                Debugger.Break();
                _logger.LogError(ex, "An error in AddMetaData");
                //todo fix tests
            }

            await _redisCacheProvider.SetCachedData(audioFile.Hash, audioFile.Id, TimeSpan.FromDays(365));
            await _audioFileRepository.AddAudioFileAsync(audioFile);

            await UpdateAudioStatus(audioFile.Id, audioFile.Status.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on processing speech");
            audioFile.Status = Status.Failed; //todo track status while processing, throw as needed
            await UpdateAudioStatus(audioFile.Id, audioFile.Status.ToString(), ex.Message);
            throw;
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
            await _redisCacheProvider.SetCachedData(hash, audioFile, TimeSpan.FromDays(365));
        }

        if (audioFile is not null)
        {
            return new MemoryStream(audioFile.Data);
        }

        var bytesCollection = await _ttsServiceFactory.Get(request.TtsApi)
            .RequestSpeechChunksAsync([request.Input], request.Voice, request.Speed, request.Model);

        if (bytesCollection.Length is not 1)
        {
            throw new Exception($"Bytes collection length is not as expected - expected  1, got: {bytesCollection.Length}");
        }

        audioFile = AudioFileBuilder.Create(bytesCollection[0].ToArray(),
            $"Sample_{request.Voice}_{request.LanguageCode}_{request.TtsApi}_{request.Speed}",
            request.Voice,
            request.LanguageCode,
            request.Speed,
            AudioType.Sample,
            ttsApiId: SharedConstants.TtsApis.Single(kv => kv.Key == request.TtsApi).Value);

        audioFile.Status = Status.Completed;

        await _redisCacheProvider.SetCachedData(hash, audioFile, TimeSpan.FromDays(365));
        await _audioFileRepository.AddAudioFileAsync(audioFile);

        return new MemoryStream(audioFile.Data);
    }

    private async Task<string> ExtractContent(IFormFile file)
    {
        var fileProcessor = _fileProcessorFactory.GetProcessor(Path.GetExtension(file.FileName)) ??
            throw new NotSupportedException("File type not supported");

        var fileText = await fileProcessor.ExtractContentAsync(file);
        return fileText;
    }

    /// <summary>
    /// Notify clients about the status update
    /// </summary>
    private async Task UpdateAudioStatus(Guid audioFileId, string status, string? errorMessage = null, int? delayMs = null)
    {
        if (delayMs is not null)
        {
            await Task.Delay(delayMs.Value);
        }

        await _hubContext.Clients.All.SendAsync(SharedConstants.AudioStatusUpdated, audioFileId.ToString(), status, errorMessage);
    }
    // todo Ensure that your repository services (_audioFileRepositoryService) handle concurrency and transaction management effectively, especially in the UpdateAudioFileAsync method.
}
