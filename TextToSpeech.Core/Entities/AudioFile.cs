using static TextToSpeech.Core.Enums;

namespace TextToSpeech.Core.Entities;

public sealed class AudioFile
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public byte[] Data { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public string Description { get; set; } = string.Empty;
    public Status Status { get; set; }
    public string Hash { get; set; } = string.Empty;
    public string Voice { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = string.Empty;
    public double Speed { get; set; }
    public AudioType Type { get; set; }

    public Guid? TtsApiId { get; set; }
    public TtsApi? TtsApi { get; set; }
}
