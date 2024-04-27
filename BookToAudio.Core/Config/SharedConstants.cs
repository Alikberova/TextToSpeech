namespace BookToAudio.Core.Config;

public static class SharedConstants
{
    public const string AppName = "BookToAudio";
    public const string AppStorage = "FileStorage";

    public const string OpenAI = "OpenAI";
    public const string Narakeet = "Narakeet";

    public static readonly string ClientProjectName = $"{AppName}.Web";
    public static readonly string ServerProjectName = $"{AppName}.Api";

    public const int ClientPort = 4000;
}
