using TextToSpeech.Core.Models;

namespace TextToSpeech.Core.Interfaces;

public interface ISpeechService
{
    Task<Guid> GetOrInitiateSpeech(TtsRequestOptions request,
        byte[] fileBytes,
        string fileName,
        string langCode,
        string ttsApi);
    Task<MemoryStream> CreateSpeechSample(TtsRequestOptions request,
        string input,
        string langCode,
        string ttsApi);
    Task ProcessSpeechAsync(TtsRequestOptions request,
        string fileText,
        string fileName,
        string langCode,
        string ttsApi,
        Guid fileId,
        string hash,
        CancellationToken cancellationToken);
}
