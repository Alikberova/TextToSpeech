namespace TextToSpeech.Infra.Constants;

public static class SharedConstants
{
    // APIs
    public const string OpenAI = "OpenAI"; // todo rename to OpenAiDisplayName 
    public const string Narakeet = "Narakeet";

    public const string OpenAiKey = "openai";
    public const string NarakeetKey = "narakeet";

    public static readonly IReadOnlyDictionary<string, Guid> TtsApis = new Dictionary<string, Guid>()
    {
        [Narakeet] = Guid.Parse("985e59c1-f5d2-4a19-853f-c7ee994ed34b"),
        [OpenAI] = Guid.Parse("6ce9e704-e049-4128-b7c6-97fe5d1716fd"),
    };

    // SignalR
    public const string AudioStatusUpdated = "AudioStatusUpdated";
    public const string AudioHubEndpoint = "/audioHub";
}
