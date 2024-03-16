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
        var envPath = ".env";
        var isDocker = Environment.CurrentDirectory is "/app";
        var isDevelopment = HostingEnvironment.IsDevelopment();
        var isRemote = HostingEnvironment.IsRemote();

        if (!isDocker)
        {
            var apiDir = PathService.GetProjectDirectory(SharedConstants.ServerProjectName);
            envPath = Path.Combine(Directory.GetParent(apiDir)!.ToString(), envPath);
            DotEnv.Load(options: new DotEnvOptions(ignoreExceptions: isDevelopment, envFilePaths: new[] { envPath }));
        }

        Console.WriteLine("envPath: " + envPath);
        Console.WriteLine("isDevelopment: " + isDevelopment);
        Console.WriteLine("isRemote: " + isRemote);
        // integration tests CurrentDirectory: /home/runner/work/BookToAudio/BookToAudio/BookToAudio.IntegrationTests/bin/Debug/net8.0
        Console.WriteLine("Environment.CurrentDirectory: " + Environment.CurrentDirectory);

        return configurationBuilder.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: isRemote, reloadOnChange: true)
            .AddEnvironmentVariables();
    }
}
