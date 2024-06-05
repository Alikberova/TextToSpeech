using TextToSpeech.Core.Services.Interfaces.Ai;
using TextToSpeech.Core.Services.Interfaces;
using TextToSpeech.Infra.Services.Ai;
using Microsoft.Extensions.DependencyInjection;
using TextToSpeech.Core.Config;
using TextToSpeech.Infra.Services.Interfaces;

namespace TextToSpeech.Infra.Services.Factories;

public class TtsServiceFactory : ITtsServiceFactory
{
    private readonly IServiceProvider _serviceProvider;

    public TtsServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

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