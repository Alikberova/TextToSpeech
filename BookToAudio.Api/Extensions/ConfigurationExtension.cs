using BookToAudio.Core;
using System.Reflection;

namespace BookToAudio.Api.Extensions;

public static class ConfigurationExtension
{
    public static IConfigurationBuilder SetConfig(this IConfigurationBuilder configurationBuilder)
    {
        //todo remove Console.WriteLine
        var isDevelopment = HostingEnvironment.IsDevelopment();
        var isWindows = HostingEnvironment.IsWindows();

        Console.WriteLine("isDevelopment: " + isDevelopment);
        Console.WriteLine("isRemote: " + isWindows);
        Console.WriteLine("Environment.CurrentDirectory: " + Environment.CurrentDirectory);

        return configurationBuilder.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: !isWindows, reloadOnChange: true)
            .AddEnvironmentVariables();
    }
}
