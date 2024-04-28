using TextToSpeech.Core.Dto.Narakeet;
using TextToSpeech.Core.Services.Interfaces;

namespace TextToSpeech.Infra.Services.Interfaces;

public interface INarakeetService : ITtsService
{
    Task<List<VoiceResponse>?> GetAvailableVoices();
}