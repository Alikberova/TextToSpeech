namespace TextToSpeech.Core.Dto;

public sealed record TranslationRequest
{
    public string Text { get; init; } = string.Empty;
    public string SourceLanguage { get; init; } = string.Empty;
    public string TargetLanguage { get; init; } = string.Empty;
}
