using Microsoft.AspNetCore.Http;
using OpenAI.Audio;

namespace BookToAudio.Core.Dto;

public sealed class SpeechRequest
{
    public string TtsApi { get; init; } = string.Empty;
    public string Voice { get; init; } = string.Empty;
    public SpeechResponseFormat ResponseFormat { get; init; }
    public double Speed { get; init; } = 1;
    public IFormFile? File { get; init; }
    public string? Input { get; init; }
    public string? Model { get; init; }
}
