using Microsoft.Extensions.Hosting;
using TextToSpeech.Core.Config;

namespace TextToSpeech.Core;

public static class HostingEnvironment
{
    public static string Current { get; private set; } = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        ?? throw new Exception("ASPNETCORE_ENVIRONMENT is not set");

    public static bool IsDevelopment()
    {
        return Current.Equals(Environments.Development, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsWindows()
    {
        return Environment.OSVersion.ToString().Contains("Windows");
    }

    public static bool IsTestMode()
    {
        return bool.TryParse(Environment.GetEnvironmentVariable(ConfigConstants.IsTestMode)?.Trim(), out var isTestMode)
            && isTestMode;
    }
}
