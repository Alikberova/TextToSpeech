using OpenAI.Audio;

namespace BookToAudio.Infa.Dto;

public class SpeechRequestDto
{
    public string Model { get; set; } = string.Empty;
    public SpeechVoice Voice { get; set; }
    public SpeechResponseFormat ResponseFormat { get; set; }
    public float? Speed { get; set; }
    public Guid FileId { get; set; }
}
