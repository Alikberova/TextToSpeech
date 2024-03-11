using BookToAudio.Core;
using BookToAudio.Core.Config;
using BookToAudio.Infra.Services.Common;
using dotenv.net;
using System.Reflection;

namespace BookToAudio.Api.Extensions;

public static class ConfigurationExtension
{
    public static IConfigurationBuilder SetConfig(this IConfigurationBuilder configurationBuilder)
    {
        var apiDir = PathService.GetProjectDirectory(SharedConstants.ServerProjectName);
        var remoteEnvFilePath = Path.Combine(Directory.GetParent(apiDir)!.ToString(), ".env");

        var isDevelopment = HostingEnvironment.IsDevelopment();
        var isRemote = apiDir.StartsWith("/home/runner") || apiDir.StartsWith("/usr");

        DotEnv.Load(options: new DotEnvOptions(ignoreExceptions: isDevelopment, envFilePaths: new[] { remoteEnvFilePath }));

        return configurationBuilder.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: isRemote, reloadOnChange: true)
            .AddEnvironmentVariables();
    }
}
