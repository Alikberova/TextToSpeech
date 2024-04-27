using BookToAudio.Core.Config;
using BookToAudio.Core.Entities;
using BookToAudio.Core.Services.Interfaces;
using BookToAudio.Infra.Services.Common;
using BookToAudio.Infra.Services.Factories;
using BookToAudio.Infra.Services.FileProcessing;
using BookToAudio.Infra.Services.Interfaces;
using BookToAudio.Infra.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using static BookToAudio.Core.Enums;

namespace BookToAudio.Infra.Services;

public sealed class SpeechService : ISpeechService
{
    private readonly ITextProcessingService _textFileService;
    private readonly ITtsServiceFactory _ttsServiceFactory;
    private readonly IAudioFileService _audioFileService;
    private readonly IPathService _pathService;
    private readonly FileProcessorFactory _fileProcessorFactory;
    private readonly IFileStorageService _fileStorageService;
    private readonly IHubContext<AudioHub> _hubContext;
    private readonly ILogger<SpeechService> _logger;
    private readonly IMetaDataService _metaDataService;

    public SpeechService(ITextProcessingService textFileService,
        ITtsServiceFactory ttsServiceFactory,
        IAudioFileService audioFileService,
        IPathService pathService,
        FileProcessorFactory fileProcessorFactory,
        IFileStorageService fileStorageService,
        IHubContext<AudioHub> hubContext,
        ILogger<SpeechService> logger,
        IMetaDataService metaDataService)
    {
        _textFileService = textFileService;
        _ttsServiceFactory = ttsServiceFactory;
        _audioFileService = audioFileService;
        _pathService = pathService;
        _fileProcessorFactory = fileProcessorFactory;
        _fileStorageService = fileStorageService;
        _hubContext = hubContext;
        _logger = logger;
        _metaDataService = metaDataService;
    }

    /// <summary>
    /// Process the speech asynchronously without waiting for it to complete to return the ID immediately to the client
    /// </summary>
    /// <param name="request"></param>
    /// <returns>ID of the newly created audio file</returns>
    /// <exception cref="ArgumentException"></exception>
    public Guid CreateSpeech(Core.Dto.SpeechRequest request)
    {
        if (request.File is null)
        {
            throw new ArgumentException(nameof(request.File));
        }

        var audioFile = new AudioFile
        {
            Id = Guid.NewGuid(),
            Status = Status.Processing,
            CreatedAt = DateTime.UtcNow,
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
            string fileText = await ExtractContent(request);

            var ttsService = _ttsServiceFactory.Get(request.TtsApi);

            var textChunks = _textFileService.SplitTextIfGreaterThan(fileText, ttsService.MaxInputLength);

            var bytesCollection = await ttsService.RequestSpeechChunksAsync(textChunks, request.Voice, request.Speed, request.Model);

            var bytes = _audioFileService.ConcatenateMp3Files(bytesCollection);

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

            await UpdateAudioStatus(audioFile.Id, audioFile.Status.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on processing speech");
            audioFile.Status = Status.Failed;
            await UpdateAudioStatus(audioFile.Id, audioFile.Status.ToString());
            throw;
        }
    }

    private async Task<string> ExtractContent(Core.Dto.SpeechRequest request)
    {
        var fileProcessor = _fileProcessorFactory.GetProcessor(Path.GetExtension(request.File?.FileName)!) ??
            throw new NotSupportedException("File type not supported");

        var fileText = await fileProcessor.ExtractContentAsync(request.File!);
        return fileText;
    }

    public async Task<MemoryStream> CreateSpeechSample(Core.Dto.SpeechRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Input))
        {
            throw new ArgumentException(nameof(request.Input));
        }

        var bytesCollection = await _ttsServiceFactory.Get(request.TtsApi)
            .RequestSpeechChunksAsync([request.Input], request.Voice, request.Speed, request.Model);

        if (bytesCollection.Length is not 1)
        {
            throw new Exception($"Bytes collection length is not as expected - expected  1, got: {bytesCollection.Length}");
        }

        return new MemoryStream(bytesCollection[0].ToArray());
    }

    /// <summary>
    /// Notify clients about the status update
    /// </summary>
    private async Task UpdateAudioStatus(Guid audioFileId, string status)
    {
        await _hubContext.Clients.All.SendAsync(SharedConstants.AudioStatusUpdated, audioFileId.ToString(), status);
    }

    // todo Ensure that your repository services (_audioFileRepositoryService) handle concurrency and transaction management effectively, especially in the UpdateAudioFileAsync method.
}
