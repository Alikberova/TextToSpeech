namespace TextToSpeech.Core.Config;

public static class SharedConstants
{
    public const string AppName = "TextToSpeech";
    public const string AppStorage = "FileStorage";
    public const string Domain = "https://texttospeech.duckdns.org";

    // APIs
    public const string OpenAI = "OpenAI";
    public const string Narakeet = "Narakeet";

    // SignalR
    public const string AudioStatusUpdated = "AudioStatusUpdated";
    public const string AudioHubEndpoint = "/audioHub";

    public static readonly string ClientProjectName = $"{AppName}.Web";
    public static readonly string ServerProjectName = $"{AppName}.Api";

    public const int ClientPort = 4000;
}
