using TextToSpeech.Core.Dto.Narakeet;

namespace TextToSpeech.Core.Interfaces.Ai;

public interface INarakeetService : ITtsService
{
    Task<List<VoiceResponse>?> GetAvailableVoices();
}