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

        Console.WriteLine("isDevelopment: " + isDevelopment);
        Console.WriteLine("Environment.CurrentDirectory: " + Environment.CurrentDirectory);

        if (string.IsNullOrWhiteSpace(configurationBuilder[ConfigConstants.AppDataPath]))
        {
            configurationBuilder[ConfigConstants.AppDataPath] = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        return configurationBuilder.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: !isDevelopment, reloadOnChange: true)
            .AddEnvironmentVariables();
    }
}
