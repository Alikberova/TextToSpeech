using Microsoft.Extensions.DependencyInjection;
using TextToSpeech.Core.Config;
using TextToSpeech.Core.Interfaces.Ai;

namespace TextToSpeech.Infra.Services.Ai;

public class TtsServiceFactory(IServiceProvider _serviceProvider) : ITtsServiceFactory
{
    public ITtsService Get(string key)
    {
        ITtsService service = key switch
        {
            SharedConstants.OpenAI => _serviceProvider.GetServices<ITtsService>().OfType<OpenAiService>().Single(),
            SharedConstants.Narakeet => _serviceProvider.GetServices<INarakeetService>().OfType<NarakeetService>().Single(),
            _ => throw new ArgumentException($"Service with key '{key}' is not registered."),
        };

        return service;
    }
}