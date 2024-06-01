namespace TextToSpeech.Core.Dto;

public sealed class TranslationRequest
{
    public string Text { get; set; } = string.Empty;
    public string SourceLanguage { get; set; } = string.Empty;
    public List<string> TargetLanguages { get; set; } = [];
    public string TargetLanguage { get; set; } = string.Empty;
}
