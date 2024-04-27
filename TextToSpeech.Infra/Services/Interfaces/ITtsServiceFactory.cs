using TextToSpeech.Core.Services.Interfaces;

namespace TextToSpeech.Infra.Services.Interfaces
{
    public interface ITtsServiceFactory
    {
        ITtsService Get(string key);
    }
}