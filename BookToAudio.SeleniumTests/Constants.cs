using BookToAudio.Core.Config;

namespace BookToAudio.SeleniumTests;

internal sealed class Constants
{
    public const string Localhost = "localhost";
    public static readonly string ClientProjectName = $"{SharedConstants.AppName}.Web";
    public static readonly string ServerProjectName = $"{SharedConstants.AppName}.Api";
    public const int ClientPort = 4000;
    public const int ServerPort = 7057;
}
