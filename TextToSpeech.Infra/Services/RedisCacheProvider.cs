using StackExchange.Redis;
using System.Text.Json;
using TextToSpeech.Infra.Services.Interfaces;

namespace TextToSpeech.Infra.Services;

public class RedisCacheProvider : IRedisCacheProvider
{
    private readonly ConnectionMultiplexer _redisConnection;

    public RedisCacheProvider(string connectionString)
    {
        _redisConnection = ConnectionMultiplexer.Connect(connectionString);
    }

    public async Task<T?> GetCachedData<T>(string key)
    {
        var db = _redisConnection.GetDatabase();
        var cachedData = await db.StringGetAsync(key);

        if (!cachedData.IsNullOrEmpty)
        {
            return JsonSerializer.Deserialize<T>(cachedData);
        }

        return default;
    }

    public async Task SetCachedData<T>(string key, T data, TimeSpan expiry)
    {
        var db = _redisConnection.GetDatabase();
        var serializedData = JsonSerializer.Serialize(data);
        await db.StringSetAsync(key, serializedData, expiry);
    }
}
