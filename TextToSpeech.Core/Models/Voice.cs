using static TextToSpeech.Core.Enums;

namespace TextToSpeech.Core.Models;

public sealed class Voice
{
    public required string Name { get; init; }
    public required string ProviderVoiceId { get; init; }
    public Language? Language { get; init; }
    public VoiceQualityTier QualityTier { get; init; }
}

public sealed class Language(string name, string languageCode)
{
    public string Name { get; init; } = name;
    public string LanguageCode { get; init; } = languageCode;
}