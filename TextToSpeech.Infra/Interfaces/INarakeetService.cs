using TextToSpeech.Core.Interfaces.Ai;
using TextToSpeech.Infra.Dto.Narakeet;

namespace TextToSpeech.Infra.Interfaces;

public interface INarakeetService : ITtsService
{
    Task<List<VoiceResponse>?> GetAvailableVoices();
}
