using Microsoft.AspNetCore.Http;
using OpenAI.Audio;

namespace BookToAudio.Core.Dto;

public sealed class SpeechRequest //todo change to init
{
    public string TtsApi { get; init; } = string.Empty;
    public string Voice { get; set; } = string.Empty;
    public SpeechResponseFormat ResponseFormat { get; set; }
    public double Speed { get; set; } = 1;
    public IFormFile? File { get; set; }
    public string? Input { get; set; }
    public string? Model { get; set; }
}
