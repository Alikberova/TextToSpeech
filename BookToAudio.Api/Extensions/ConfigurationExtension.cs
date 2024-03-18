using BookToAudio.Core;
using BookToAudio.Core.Config;
using System.Reflection;

namespace BookToAudio.Api.Extensions;

public static class ConfigurationExtension
{
    public static IConfigurationBuilder SetConfig(this IConfigurationManager configurationBuilder)
    {
        //todo remove Console.WriteLine
        var isDevelopment = HostingEnvironment.IsDevelopment();
        var isWindows = HostingEnvironment.IsWindows();

        Console.WriteLine("isDevelopment: " + isDevelopment);
        Console.WriteLine("isRemote: " + isWindows);
        Console.WriteLine("Environment.CurrentDirectory: " + Environment.CurrentDirectory);

        if (string.IsNullOrWhiteSpace(configurationBuilder[ConfigConstants.AppDataPath]))
        {
            configurationBuilder[ConfigConstants.AppDataPath] = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        return configurationBuilder.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: !isWindows, reloadOnChange: true)
            .AddEnvironmentVariables();
    }
}
