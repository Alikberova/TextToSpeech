namespace BookToAudio.Core.Entities;

public class AudioFile
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public byte[] Data { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public string Description { get; set; } = string.Empty;
}
