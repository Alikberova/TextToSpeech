namespace BookToAudio.Core.Dto;

public sealed record class EmailRequest
{
    public string Name { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
