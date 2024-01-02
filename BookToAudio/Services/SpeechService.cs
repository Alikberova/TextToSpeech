using BookToAudio.Core;
using BookToAudio.Core.Repositories;
using BookToAudio.Core.Services.Interfaces;
using BookToAudio.Infra.Services;
using OpenAI.Audio;

namespace BookToAudio.Services;

public class SpeechService
{
    private readonly ITextFileService _textFileService;
    private readonly IOpenAiService _openAiService;
    private readonly IAudioFileService _audioFileService;
    private readonly IPathService _pathService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IAudioFileRepositoryService _audioFileRepositoryService;

    public SpeechService(ITextFileService textFileService,
        IOpenAiService openAiService,
        IAudioFileService audioFileService,
        IPathService pathService,
        IFileStorageService fileStorageService,
        IAudioFileRepositoryService audioFileRepositoryService)
    {
        _textFileService = textFileService;
        _openAiService = openAiService;
        _audioFileService = audioFileService;
        _pathService = pathService;
        _fileStorageService = fileStorageService;
        _audioFileRepositoryService = audioFileRepositoryService;
    }
    

    internal async Task CreateSpeechAsync(Infra.Dto.SpeechRequest request)
    {
        var fileText = await _fileStorageService.RetrieveFileTextAsync(request.FileId.ToString());

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
