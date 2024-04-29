namespace TextToSpeech.Core.Dto.Narakeet;

public class BuildTaskStatus
{
    public string Message { get; set; } = string.Empty;
    public int Percent { get; set; }
    public bool Succeeded { get; set; }
    public bool Finished { get; set; }
    public string Result { get; set; } = string.Empty;
}
