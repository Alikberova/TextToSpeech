namespace TextToSpeech.Core.Dto.Narakeet;

public sealed record AudioTaskRequest
{
    public string Voice { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public string Format { get; init; } = string.Empty;
    public double Speed { get; init; }
}
