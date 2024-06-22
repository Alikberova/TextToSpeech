using TextToSpeech.Core.Config;
using Microsoft.Extensions.Configuration;
using TextToSpeech.Core.Interfaces;

namespace TextToSpeech.Infra.Services.Common;

public sealed class PathService(IConfiguration _configuration) : IPathService
{
    public string GetFilePathInFileStorage(string fileName)
    {
        return Path.Combine(GetFileStoragePath(), fileName);
    }

    public string GetFileStoragePath()
    {
        var path = Path.Combine(_configuration.GetValue<string>(ConfigConstants.AppDataPath)!,
            SharedConstants.AppStorage);

        Directory.CreateDirectory(path);

        return path;
    }
}
