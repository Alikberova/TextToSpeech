using TextToSpeech.Core.Dto;

namespace TextToSpeech.Core.Interfaces;

public interface ISpeechService
{
    Task<Guid> CreateSpeech(SpeechRequest request);
    Task<MemoryStream> CreateSpeechSample(SpeechRequest request);
    Task ProcessSpeechAsync(SpeechRequest request,
        Guid fileId,
        string fileText,
        string hash,
        CancellationToken cancellationToken);
}
