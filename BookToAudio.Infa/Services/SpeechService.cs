using BookToAudio.Core;
using BookToAudio.Core.Entities;
using BookToAudio.Core.Services.Interfaces;
using BookToAudio.Infra.Services.Common;
using BookToAudio.Infra.Services.Factories;
using BookToAudio.Infra.Services.FileProcessing;
using BookToAudio.RealTime;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using static BookToAudio.Core.Enums;

namespace BookToAudio.Infra.Services;

public class SpeechService : ISpeechService
{
    private readonly ITextProcessingService _textFileService;
    private readonly IOpenAiService _openAiService;
    private readonly IAudioFileService _audioFileService;
    private readonly IPathService _pathService;
    private readonly FileProcessorFactory _fileProcessorFactory;
    private readonly IFileStorageService _fileStorageService;
    private readonly IHubContext<AudioHub> _hubContext;

    public SpeechService(ITextProcessingService textFileService,
        IOpenAiService openAiService,
        IAudioFileService audioFileService,
        IPathService pathService,
        FileProcessorFactory fileProcessorFactory,
        IFileStorageService fileStorageService,
        IHubContext<AudioHub> hubContext)
    {
        _textFileService = textFileService;
        _openAiService = openAiService;
        _audioFileService = audioFileService;
        _pathService = pathService;
        _fileProcessorFactory = fileProcessorFactory;
        _fileStorageService = fileStorageService;
        _hubContext = hubContext;
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

            var maxLength = 4096;

            if (HostingEnvironment.IsDevelopment())
            {
                maxLength = 150;
            }

            var textChunks = _textFileService.SplitTextIfGreaterThan(fileText, maxLength);

            var bytesCollection = await _openAiService.RequestSpeechChunksAsync(textChunks, request.Model, request.Voice, request.Speed);

            var bytes = _audioFileService.ConcatenateMp3Files(bytesCollection);

            var localFilePath = _pathService.CreateFileStorageFilePath($"{audioFile.Id}.mp3");

            await File.WriteAllBytesAsync(localFilePath, bytes);

            audioFile.Status = Status.Completed;
            audioFile.Data = bytes;

            await UpdateAudioStatus(audioFile.Id, audioFile.Status.ToString());
        }
        catch (Exception ex)
        {
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

        var bytesCollection = await _openAiService.RequestSpeechChunksAsync([request.Input], request.Model, request.Voice, request.Speed);

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
        await _hubContext.Clients.All.SendAsync("AudioStatusUpdated", audioFileId.ToString(), status);
    }

    // todo Ensure that your repository services (_audioFileRepositoryService) handle concurrency and transaction management effectively, especially in the UpdateAudioFileAsync method.
}
