namespace BookToAudio.Core.Dto;

public sealed class EmailRequest
{
    public string Name { get; set; }
    public string UserEmail { get; set; }
    public string Message { get; set; }
}
