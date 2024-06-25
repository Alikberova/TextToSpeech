using Microsoft.AspNetCore.Http;
using OpenAI.Audio;

namespace TextToSpeech.Core.Dto;

public sealed record SpeechRequest
{
    public string TtsApi { get; init; } = string.Empty;
    public string Voice { get; init; } = string.Empty;
    public SpeechResponseFormat ResponseFormat { get; init; }
    public double Speed { get; init; } = 1;
    public string LanguageCode { get; init; } = string.Empty;
    public IFormFile? File { get; init; }
    public string? Input { get; init; }
    public string? Model { get; init; }
}
