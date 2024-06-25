using Microsoft.Extensions.Hosting;

namespace TextToSpeech.Core;

public static class HostingEnvironment
{
    public static bool IsDevelopment()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development;
    }
}
