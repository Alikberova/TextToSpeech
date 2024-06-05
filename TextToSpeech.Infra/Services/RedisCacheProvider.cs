using StackExchange.Redis;
using System.Text.Json;
using TextToSpeech.Infra.Services.Interfaces;

namespace TextToSpeech.Infra.Services;

public class RedisCacheProvider : IRedisCacheProvider
{
    private readonly ConnectionMultiplexer? _redisConnection;

    public RedisCacheProvider(string connectionString)
    {
        try
        {
            _redisConnection = ConnectionMultiplexer.Connect(connectionString);
        }
        catch (RedisConnectionException)
        {
        }
    }

    public async Task<T?> GetCachedData<T>(string key)
    {
        if (_redisConnection is null || !_redisConnection.IsConnected)
        {
            return default;
        }

        var db = _redisConnection.GetDatabase();
        var cachedData = await db.StringGetAsync(key);

        if (!cachedData.IsNullOrEmpty)
        {
            return JsonSerializer.Deserialize<T>(cachedData!);
        }

        return default;
    }

    public async Task SetCachedData<T>(string key, T data, TimeSpan expiry)
    {
        if (_redisConnection is null || !_redisConnection.IsConnected)
        {
            return;

        }
        var db = _redisConnection.GetDatabase();
        var serializedData = JsonSerializer.Serialize(data);
        await db.StringSetAsync(key, serializedData, expiry);
    }
}
