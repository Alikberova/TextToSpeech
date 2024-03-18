using Microsoft.Extensions.Hosting;

namespace BookToAudio.Core;

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
}
