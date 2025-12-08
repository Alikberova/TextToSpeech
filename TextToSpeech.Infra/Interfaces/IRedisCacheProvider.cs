namespace TextToSpeech.Infra.Interfaces;

public interface IRedisCacheProvider
{
    Task<T?> GetCachedData<T>(string key);
    Task SetCachedData<T>(string key, T data, TimeSpan expiry);
}