using BookToAudio.Core.Services.Interfaces.Ai;
using BookToAudio.Core.Services.Interfaces;
using BookToAudio.Infra.Services.Ai.Narakeet;
using Microsoft.Extensions.DependencyInjection;
using BookToAudio.Core.Config;
using BookToAudio.Infra.Services.Interfaces;

namespace BookToAudio.Infra.Services.Factories;

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
            SharedConstants.Narakeet => _serviceProvider.GetServices<ITtsService>().OfType<NarakeetService>().Single(),
            _ => throw new ArgumentException($"Service with key '{key}' is not registered."),
        };

        return service;
    }
}

