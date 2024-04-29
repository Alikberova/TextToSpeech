namespace TextToSpeech.Core.Dto.Narakeet;

public sealed class AudioTaskRequest
{
    public string Voice { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public double Speed { get; set; }
}
