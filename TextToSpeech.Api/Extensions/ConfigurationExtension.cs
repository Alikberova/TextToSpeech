using System.Reflection;
using TextToSpeech.Infra.Config;

namespace TextToSpeech.Api.Extensions;

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
