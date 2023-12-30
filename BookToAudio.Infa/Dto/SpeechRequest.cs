using OpenAI.Audio;

namespace BookToAudio.Infra.Dto;

public class SpeechRequest
{
    public string Model { get; set; } = string.Empty;
    public SpeechVoice Voice { get; set; }
    public SpeechResponseFormat ResponseFormat { get; set; }
    public float? Speed { get; set; }
    public Guid FileId { get; set; }
}
