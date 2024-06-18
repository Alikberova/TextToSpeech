using static TextToSpeech.Core.Enums;

namespace TextToSpeech.Core.Entities;

public sealed class AudioFile
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public byte[] Data { get; set; } = [];
    public DateTime CreatedAt { get; init; }
    public string Description { get; init; } = string.Empty;
    public Status Status { get; set; }
    public string Hash { get; set; } = string.Empty;
    public string Voice { get; init; } = string.Empty;
    public string LanguageCode { get; init; } = string.Empty;
    public double Speed { get; init; }
    public AudioType Type { get; init; }

    public Guid? TtsApiId { get; set; }
    public TtsApi? TtsApi { get; set; }
}
