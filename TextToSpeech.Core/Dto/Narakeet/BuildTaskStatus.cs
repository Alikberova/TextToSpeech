namespace TextToSpeech.Core.Dto.Narakeet;

public class BuildTaskStatus
{
    public string message { get; set; } = string.Empty;
    public int percent { get; set; }
    public bool succeeded { get; set; }
    public bool finished { get; set; }
    public string result { get; set; } = string.Empty;
}
