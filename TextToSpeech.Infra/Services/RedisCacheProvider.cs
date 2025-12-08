using StackExchange.Redis;
using System.Text.Json;
using TextToSpeech.Infra.Interfaces;

namespace TextToSpeech.Infra.Services;

public sealed class RedisCacheProvider(IConnectionMultiplexer redisConnection) : IRedisCacheProvider
{
    public async Task<T?> GetCachedData<T>(string key)
    {
        var db = redisConnection.GetDatabase();
        var cachedData = await db.StringGetAsync(key);

        if (!cachedData.IsNullOrEmpty)
        {
            return JsonSerializer.Deserialize<T>((string)cachedData!);
        }

        return default;
    }

    public async Task SetCachedData<T>(string key, T data, TimeSpan expiry)
    {
        var db = redisConnection.GetDatabase();
        var serializedData = JsonSerializer.Serialize(data);

        await db.StringSetAsync(key, serializedData, expiry);
    }
}
