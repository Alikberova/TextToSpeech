using StackExchange.Redis;
using System.Text.Json;
using TextToSpeech.Core.Interfaces;

namespace TextToSpeech.Infra.Services;

public sealed class RedisCacheProvider : IRedisCacheProvider
{
    private readonly IConnectionMultiplexer _redisConnection;

    public RedisCacheProvider(IConnectionMultiplexer redisConnection)
    {
        _redisConnection = redisConnection;
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

    public bool IsConnected()
    {
        return _redisConnection is not null && _redisConnection.IsConnected;
    }
}
