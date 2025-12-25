using StackExchange.Redis;
using System.Text.Json;
using TextToSpeech.Infra.Interfaces;

namespace TextToSpeech.Infra.Services;

public sealed class RedisCacheProvider(IConnectionMultiplexer redisConnection) : IRedisCacheProvider
{
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromDays(7);

    public async Task<byte[]?> GetBytes(string key)
    {
        var db = redisConnection.GetDatabase();
        var value = await db.StringGetAsync(key);

        if (value.IsNullOrEmpty)
        {
            return null;
        }

        return (byte[])value!;
    }

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

    public Task SetBytes(string key, byte[] data, TimeSpan? expiry = null)
    {
        var db = redisConnection.GetDatabase();
        return db.StringSetAsync(key, data, expiry ?? DefaultExpiry);
    }

    public Task SetCachedData<T>(string key, T data, TimeSpan? expiry = null)
    {
        var db = redisConnection.GetDatabase();
        var serializedData = JsonSerializer.Serialize(data);

        return db.StringSetAsync(key, serializedData, expiry ?? DefaultExpiry);
    }
}
