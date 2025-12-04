using TextToSpeech.Core.Models;

namespace TextToSpeech.Api;

public sealed record SpeechRequest
{
    public required string TtsApi { get; init; }
    public string? LanguageCode { get; init; }
    public IFormFile? File { get; init; }
    public string? Input { get; init; }
    public required TtsRequestOptions TtsRequestOptions { get; init; }
}