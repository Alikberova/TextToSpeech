namespace TextToSpeech.Core;

public sealed record ProgressReport
{
    public required Guid FileId { get; init; }
    public required int ProgressPercentage { get; init; }
}
