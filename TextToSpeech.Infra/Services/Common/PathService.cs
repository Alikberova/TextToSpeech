using TextToSpeech.Core.Config;
using Microsoft.Extensions.Configuration;
using TextToSpeech.Core.Interfaces;

namespace TextToSpeech.Infra.Services.Common;

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
}
