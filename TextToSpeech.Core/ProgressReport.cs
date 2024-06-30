namespace TextToSpeech.Core;

public sealed class ProgressReport
{
    public Guid FileId { get; set; }
    public int ProgressPercentage { get; set; }
}
