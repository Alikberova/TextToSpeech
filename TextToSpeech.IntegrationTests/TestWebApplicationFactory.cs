using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Testcontainers.PostgreSql;
using TextToSpeech.Infra.Config;
using TextToSpeech.Infra.Interfaces;

namespace TextToSpeech.IntegrationTests;

public class TestWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram>, IAsyncLifetime where TProgram : class
{
    private const string DbConnectionEnv = "ConnectionStrings__DefaultConnection";

    private readonly PostgreSqlContainer? _dbContainer;
    private readonly bool _hasExternalDbContainer =
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(DbConnectionEnv));

    public HttpClient HttpClient { get; private set; } = null!;

    public TestWebApplicationFactory()
    {
        if (!_hasExternalDbContainer)
        {
            _dbContainer = new PostgreSqlBuilder()
                .WithCleanUp(true)
                .Build();
        }
    }

    public async Task InitializeAsync()
    {
        if (!_hasExternalDbContainer)
        {
            await _dbContainer!.StartAsync();
        }

        HttpClient = CreateClient();
    }

    public new async Task DisposeAsync()
    {
        if (_dbContainer is not null)
        {
            await _dbContainer.DisposeAsync();
        }

        HttpClient.Dispose();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == null)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", Environments.Development);
        }

        Environment.SetEnvironmentVariable(ConfigConstants.IsTestMode, "true");

        if (!_hasExternalDbContainer)
        {
            Environment.SetEnvironmentVariable(DbConnectionEnv , _dbContainer!.GetConnectionString());
        }

        builder.ConfigureTestServices(services =>
        {
            services.AddScoped(_ => Mock.Of<IRedisCacheProvider>());
        });
    }
}