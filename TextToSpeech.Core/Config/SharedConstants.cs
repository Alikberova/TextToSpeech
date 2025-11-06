namespace TextToSpeech.Core.Config;

public static class SharedConstants
{
    public const string AppName = "TextToSpeech";
    public const string AppStorage = "FileStorage";
    public const string Domain = "https://texttospeech.duckdns.org";

    // APIs
    public const string OpenAI = "OpenAI"; // todo rename to OpenAiDisplayName 
    public const string Narakeet = "Narakeet";

    public const string OpenAiKey = "openai";
    public const string NarakeetKey = "narakeet";

    public static readonly Dictionary<string, Guid> TtsApis = new()
    {
        [Narakeet] = Guid.Parse("985e59c1-f5d2-4a19-853f-c7ee994ed34b"),
        [OpenAI] = Guid.Parse("6ce9e704-e049-4128-b7c6-97fe5d1716fd"),
    };

    // SignalR
    public const string AudioStatusUpdated = "AudioStatusUpdated";
    public const string AudioHubEndpoint = "/audioHub";

    public static readonly string ClientProjectName = $"{AppName}.Web";
    public static readonly string ServerProjectName = $"{AppName}.Api";

    public const int ClientPort = 4000;
}
