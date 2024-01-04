using BookToAudio.Core;
using BookToAudio.Core.Repositories;
using BookToAudio.Core.Services.Interfaces;
using BookToAudio.Infra.Services;
using BookToAudio.Infra.Services.Common;
using BookToAudio.Infra.Services.Factories;
using BookToAudio.Infra.Services.FileProcessing;
using OpenAI.Audio;

namespace BookToAudio.Services;

public class SpeechService
{
    private readonly ITextProcessingService _textFileService;
    private readonly IOpenAiService _openAiService;
    private readonly IAudioFileService _audioFileService;
    private readonly IPathService _pathService;
    private readonly FileProcessorFactory _fileProcessorFactory;
    private readonly IAudioFileRepositoryService _audioFileRepositoryService;

    public SpeechService(ITextProcessingService textFileService,
        IOpenAiService openAiService,
        IAudioFileService audioFileService,
        IPathService pathService,
        FileProcessorFactory fileProcessorFactory,
        IAudioFileRepositoryService audioFileRepositoryService)
    {
        _textFileService = textFileService;
        _openAiService = openAiService;
        _audioFileService = audioFileService;
        _pathService = pathService;
        _fileProcessorFactory = fileProcessorFactory;
        _audioFileRepositoryService = audioFileRepositoryService;
    }
    

    internal async Task CreateSpeechAsync(Infra.Dto.SpeechRequest request)
    {
        var fileProcessor = _fileProcessorFactory.GetProcessor(request.FileType) ??
            throw new NotSupportedException("File type not supported");

        var fileText = await fileProcessor.ExtractContentAsync(request.FileId.ToString());

        var maxLength = 4096;

        if (HostingEnvironment.IsDevelopment())
        {
            maxLength = 150;
        }

        var textChunks = _textFileService.SplitTextIfGreaterThan(fileText, maxLength);

        var newRequest = new SpeechRequest(fileText, request.Model, request.Voice, request.ResponseFormat, request.Speed);

        ReadOnlyMemory<byte>[] bytesCollection = await _openAiService.RequestSpeechChunksAsync(newRequest, textChunks);

        var bytes = _audioFileService.ConcatenateMp3Files(bytesCollection);

        if (HostingEnvironment.IsDevelopment())
        {
            string filePath = _pathService.GetFileStorageFilePath($"{DateTime.Now.Minute}.mp3");
            await File.WriteAllBytesAsync(filePath, bytes);
        }

        await _audioFileRepositoryService.AddAudioFileAsync(bytes, request.FileId);
        // TODO: audio metadata
    }
}
