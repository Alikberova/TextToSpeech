using BookToAudio.Core.Config;
using Microsoft.Extensions.Configuration;

namespace BookToAudio.Infra.Services;

public interface IPathService
{
    string GetFileStorageFilePath(string fileName);
    string GetFileStoragePath();
}

public class PathService : IPathService
{
    private readonly IConfiguration _configuration;

    public PathService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetFileStorageFilePath(string fileName)
    {
        return Path.Combine(GetFileStoragePath(), fileName);
    }

    public string GetFileStoragePath()
    {
        return _configuration.GetValue<string>(ConfigConstants.FileStoragePath) ??
            throw new ArgumentNullException(ConfigConstants.FileStoragePath);
    }
}
