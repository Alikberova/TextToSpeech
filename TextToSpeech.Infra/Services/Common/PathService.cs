using TextToSpeech.Core.Config;
using Microsoft.Extensions.Configuration;
using TextToSpeech.Core.Interfaces;

namespace TextToSpeech.Infra.Services.Common;

public sealed class PathService(IConfiguration _configuration) : IPathService
{
    public string ResolveFilePathForStorage(Guid fileId, string fileExtension = "mp3")
    {
        var path = Path.Combine(GetFileStoragePath(), $"{fileId}.{fileExtension}");

        var fullPath = Path.GetFullPath(path);
        var basePath = Path.GetFullPath(GetFileStoragePath());

        if (!fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Unsafe path detected.");
        }

        return path;
    }

    public string GetFileStoragePath()
    {
        var path = Path.Combine(_configuration.GetValue<string>(ConfigConstants.AppDataPath)!,
            SharedConstants.AppStorage);

        Directory.CreateDirectory(path);

        return path;
    }
}
