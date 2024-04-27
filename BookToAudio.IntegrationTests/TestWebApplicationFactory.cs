using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookToAudio.IntegrationTests;

public class TestWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    private Action<IServiceCollection> _configureTestServices = null!;
    public bool RunRealApiTests { get; init; }

    public TestWebApplicationFactory()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        RunRealApiTests = configuration.GetValue<bool>("RunRealApiTests");
    }

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