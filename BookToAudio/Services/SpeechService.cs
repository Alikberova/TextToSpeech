using BookToAudio.Core;
using BookToAudio.Core.Entities;
using BookToAudio.Core.Repositories;
using BookToAudio.Core.Services.Interfaces;
using BookToAudio.Infra.Services;
using BookToAudio.Infra.Services.Common;
using BookToAudio.Infra.Services.Factories;
using BookToAudio.Infra.Services.FileProcessing;
using OpenAI.Audio;
using static BookToAudio.Core.Enums;

namespace BookToAudio.Services;

public class SpeechService
{
    private readonly ITextProcessingService _textFileService;
    private readonly IOpenAiService _openAiService;
    private readonly IAudioFileService _audioFileService;
    private readonly IPathService _pathService;
    private readonly FileProcessorFactory _fileProcessorFactory;
    private readonly IAudioFileRepositoryService _audioFileRepositoryService;
    private readonly IFileStorageService _fileStorageService;

    public SpeechService(ITextProcessingService textFileService,
        IOpenAiService openAiService,
        IAudioFileService audioFileService,
        IPathService pathService,
        FileProcessorFactory fileProcessorFactory,
        IAudioFileRepositoryService audioFileRepositoryService,
        IFileStorageService fileStorageService)
    {
        _textFileService = textFileService;
        _openAiService = openAiService;
        _audioFileService = audioFileService;
        _pathService = pathService;
        _fileProcessorFactory = fileProcessorFactory;
        _audioFileRepositoryService = audioFileRepositoryService;
        _fileStorageService = fileStorageService;
    }
    
    internal async Task<Guid> CreateSpeechAsync(Infra.Dto.SpeechRequest request)
    {
        var filedata = request.File?.FileName.Split('.');

        if (filedata?.Length is not 2)
        {
            throw new ArgumentException("Couldn't get the extension type from file because expected that file name will contain 1 dot");
        }

        var fileProcessor = _fileProcessorFactory.GetProcessor(filedata[1]) ??
            throw new NotSupportedException("File type not supported");

        //store file on the machine; save to db later todo
        var fileId = await _fileStorageService.StoreFileAsync(request.File!);

        var fileText = await fileProcessor.ExtractContentAsync(fileId.ToString());

        var maxLength = 4096;

        if (HostingEnvironment.IsDevelopment())
        {
            maxLength = 150;
        }

        var textChunks = _textFileService.SplitTextIfGreaterThan(fileText, maxLength);

        var newRequest = new SpeechRequest(fileText, request.Model, request.Voice, request.ResponseFormat, request.Speed);

        var audioFile = new AudioFile
        {
            Id = fileId,
            CreatedAt = DateTime.UtcNow,
            Status = Status.Processing
        };

        // MIGRATION IS NOT DONE YET; THIS WILL THROW AN ERROR
        await _audioFileRepositoryService.AddAudioFileAsync(audioFile);

        var bytesCollection = await _openAiService.RequestSpeechChunksAsync(newRequest, textChunks);

        var bytes = _audioFileService.ConcatenateMp3Files(bytesCollection);

        if (HostingEnvironment.IsDevelopment())
        {
            string filePath = _pathService.GetFileStorageFilePath($"{DateTime.Now.Minute}.mp3");
            await File.WriteAllBytesAsync(filePath, bytes);
        }

        audioFile.Status = Status.Completed;
        audioFile.Data = bytes;

        await _audioFileRepositoryService.UpdateAudioFileAsync(audioFile);

        return fileId;
        // TODO: audio metadata
    }
}
