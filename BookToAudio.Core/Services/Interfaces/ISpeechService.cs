using BookToAudio.Core.Dto;

namespace BookToAudio.Core.Services.Interfaces;

public interface ISpeechService
{
    Guid CreateSpeech(SpeechRequest request);
    Task<MemoryStream> CreateSpeechSample(SpeechRequest request);
}
