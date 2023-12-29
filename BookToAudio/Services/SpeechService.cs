using BookToAudio.Core;
using BookToAudio.Core.Config;
using BookToAudio.Core.Services.Interfaces;
using BookToAudio.Infa.Services;
using OpenAI.Audio;

namespace BookToAudio.Services;

public class SpeechService
{
    private readonly ITextFileService _textFileService;
    private readonly IOpenAiService _openAiService;
    private readonly IAudioFileService _audioFileService;
    private readonly IConfiguration _configuration;

    public SpeechService(ITextFileService textFileService,
        IOpenAiService openAiService,
        IAudioFileService audioFileService,
        IConfiguration configuration)
    {
        _textFileService = textFileService;
        _openAiService = openAiService;
        _audioFileService = audioFileService;
        _configuration = configuration;
    }
    

    internal async Task CreateSpeechAsync(SpeechRequest request)
    {
        var maxLength = 4096;

        if (HostingEnvironment.IsDevelopment())
        {
            maxLength = 150;
        }

        var textChunks = _textFileService.SplitTextIfGreaterThan(request.Input, maxLength);

        ReadOnlyMemory<byte>[] bytes = await _openAiService.RequestSpeechChunksAsync(request, textChunks);

        var result = _audioFileService.ConcatenateMp3Files(bytes);

        if (HostingEnvironment.IsDevelopment())
        {
            var storagePath = _configuration.GetValue<string>(ConfigConstants.FileStoragePath) ??
                throw new ArgumentNullException(ConfigConstants.FileStoragePath);

            string filePath = Path.Combine(storagePath, $"{DateTime.Now.Minute}.mp3"); ;
            await File.WriteAllBytesAsync(filePath, result);
        }

        // TODO: Call DB from service to save the audio metadata, etc.
    }
}
