namespace TextToSpeech.Infra.Config;

public static class ConfigConstants
{
    public static class SectionNames
    {
        public const string EmailConfig = "EmailConfig";
        public const string NarakeetConfig = "NarakeetConfig";
        public const string JwtConfig = "JwtConfig";
    }

    public const string AppDataPath = "AppDataPath";
    public const string OpenAiApiKey = "OPENAI_API_KEY";
    public const string ElevenLabsApiKey = "ELEVENLABS_API_KEY";
    public const string IsTestMode = "IsTestMode";
}
