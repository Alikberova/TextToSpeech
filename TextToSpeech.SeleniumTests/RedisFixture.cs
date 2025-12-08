using StackExchange.Redis;
using Testcontainers.Redis;
using TextToSpeech.Infra.Constants;
using TextToSpeech.Infra.Services;
using static TextToSpeech.Infra.TestData;

namespace TextToSpeech.SeleniumTests;

public sealed class RedisFixture : IAsyncLifetime
{
    private readonly RedisContainer? _redisContainer;
    private string? ConnectionString { get; set; } = Environment.GetEnvironmentVariable("ConnectionStrings__Redis");
    private const int PublicPort = 6379;

    public RedisFixture()
    {
        // CI environment will provide its own Redis instance
        if (!string.IsNullOrWhiteSpace(ConnectionString))
        {
            return;
        }

        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithCleanUp(true)
            .WithName("selenium-redis")
            .WithPortBinding(PublicPort, 6379)
            .Build();
    }

    public async Task InitializeAsync()
    {
        if (!string.IsNullOrWhiteSpace(ConnectionString))
        {
            await SeedAsync();
            return;
        }

        await _redisContainer!.StartAsync();

        ConnectionString = $"{_redisContainer.Hostname}:{PublicPort}";

        await SeedAsync();
    }

    private async Task SeedAsync()
    {
        var mux = await ConnectionMultiplexer.ConnectAsync(ConnectionString!);

        var cacheProvider = new RedisCacheProvider(mux);

        await cacheProvider.SetCachedData(CacheKeys.Voices(SharedConstants.OpenAiKey),
            OpenAiVoices.All, TimeSpan.FromDays(1));

        await cacheProvider.SetCachedData(CacheKeys.Voices(SharedConstants.NarakeetKey),
            NarakeetVoices.All, TimeSpan.FromDays(1));
    }

    public async Task DisposeAsync()
    {
        if (_redisContainer is null)
        {
            return;
        }

        await _redisContainer.DisposeAsync();
    }
}
