using TextToSpeech.Core.Dto;

namespace TextToSpeech.Core.Services.Interfaces;

public interface ISpeechService
{
    Guid CreateSpeech(SpeechRequest request);
    Task<MemoryStream> CreateSpeechSample(SpeechRequest request);
}
