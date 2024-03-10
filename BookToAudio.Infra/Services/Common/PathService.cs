using BookToAudio.Core.Config;
using Microsoft.Extensions.Configuration;

namespace BookToAudio.Infra.Services.Common;

public interface IPathService
{
    string GetFileStorageFilePath(string fileName);
    string GetFileStoragePath();
}

public sealed class PathService : IPathService
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
        return Path.Combine(_configuration.GetValue<string>(ConfigConstants.AppDataPath)!,
            SharedConstants.AppStorage);
    }

    public static string GetProjectDirectory(string projectName)
    {
        var path = Directory.GetCurrentDirectory();

        while (path != null && !Directory.Exists(Path.Combine(path, projectName)))
        {
            path = Directory.GetParent(path)?.FullName;
        }

        path = Path.Combine(path!, projectName);

        return path;
    }
}
