namespace TextToSpeech.Core.Entities;

public class Translation
{
    public Guid Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public string SourceLanguage { get; init; } = string.Empty;
    public string TargetLanguage { get; init; } = string.Empty;
    public string OriginalText { get; init; } = string.Empty;
    public string TranslatedText { get; init; } = string.Empty;
}
