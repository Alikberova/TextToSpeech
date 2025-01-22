using Microsoft.Extensions.Hosting;
using TextToSpeech.Core.Config;

namespace TextToSpeech.Core;

public static class HostingEnvironment
{
    public static bool IsDevelopment()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development;
    }

    public static bool IsWindows()
    {
        return Environment.OSVersion.ToString().Contains("Windows");
    }

    public static bool IsTestMode()
    {
        var re = Environment.GetEnvironmentVariable(ConfigConstants.IsTestMode);
        return bool.TryParse(Environment.GetEnvironmentVariable(ConfigConstants.IsTestMode)?.Trim(), out var isTestMode) && isTestMode;
    }
}
