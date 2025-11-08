using Microsoft.Extensions.DependencyInjection;
using TextToSpeech.Core;
using TextToSpeech.Core.Interfaces.Ai;
using TextToSpeech.Infra.Constants;

namespace TextToSpeech.Infra.Services.Ai;

public sealed class TtsServiceFactory(IServiceProvider _serviceProvider) : ITtsServiceFactory
{
    public ITtsService Get(string key)
    {
        if (HostingEnvironment.IsTestMode())
        {
            return _serviceProvider.GetRequiredService<SimulatedTtsService>();
        }

        ITtsService service = key switch
        {
            SharedConstants.OpenAiKey => _serviceProvider.GetServices<ITtsService>().OfType<OpenAiService>().Single(),
            SharedConstants.NarakeetKey => _serviceProvider.GetServices<INarakeetService>().OfType<NarakeetService>().Single(),
            _ => throw new ArgumentException($"Service with key '{key}' is not registered."),
        };

        return service;
    }
}