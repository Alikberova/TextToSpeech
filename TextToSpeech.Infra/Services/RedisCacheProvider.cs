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
        catch (RedisConnectionException ex)
        {
            Console.WriteLine("RedisConnectionException Message: " + ex.Message);
            Console.WriteLine("RedisConnectionException StackTrace: " + ex.StackTrace);
        }
    }

    public async Task<T?> GetCachedData<T>(string key)
    {
        Console.WriteLine("Trying to get cache from redis.");
        Console.WriteLine("_redisConnection: " + _redisConnection);
        Console.WriteLine("_redisConnection?.IsConnected: " + _redisConnection?.IsConnected);

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
        Console.WriteLine("Trying to set cache to redis.");
        Console.WriteLine("_redisConnection: " + _redisConnection);
        Console.WriteLine("_redisConnection?.IsConnected: " + _redisConnection?.IsConnected);

        if (_redisConnection is null || !_redisConnection.IsConnected)
        {
            return;

        }
        var db = _redisConnection.GetDatabase();
        var serializedData = JsonSerializer.Serialize(data);
        await db.StringSetAsync(key, serializedData, expiry);
    }
}
