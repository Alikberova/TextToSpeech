namespace TextToSpeech.Infra.Interfaces;

public interface IRedisCacheProvider
{
    Task<byte[]?> GetBytes(string key);
    Task<T?> GetCachedData<T>(string key);
    Task SetBytes(string key, byte[] data, TimeSpan expiry);
    Task SetCachedData<T>(string key, T data, TimeSpan expiry);
}