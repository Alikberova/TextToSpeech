using TextToSpeech.Core.Models;

namespace TextToSpeech.Core.Dto;

public sealed record TtsRequestOptions
{
    public string? Model { get; init; }
    public required string Voice { get; init; }
    public required double Speed { get; init; }
    public required SpeechResponseFormat ResponseFormat { get; init; }
}