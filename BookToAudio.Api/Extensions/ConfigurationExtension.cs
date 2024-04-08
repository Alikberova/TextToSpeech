using BookToAudio.Core.Config;
using System.Reflection;

namespace BookToAudio.Api.Extensions;

public static class ConfigurationExtension
{
    public static IConfigurationBuilder SetConfig(this IConfigurationManager configurationBuilder)
    {
        if (string.IsNullOrWhiteSpace(configurationBuilder[ConfigConstants.AppDataPath]))
        {
            configurationBuilder[ConfigConstants.AppDataPath] = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        return configurationBuilder.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
    }
}
