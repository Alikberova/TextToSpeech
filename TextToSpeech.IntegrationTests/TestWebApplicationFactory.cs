using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace TextToSpeech.IntegrationTests;

public class TestWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    private Action<IServiceCollection> _configureTestServices = null!;

    public void ConfigureTestServices(Action<IServiceCollection> configureServices)
    {
        _configureTestServices = configureServices;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            _configureTestServices?.Invoke(services);
        });
    }
}