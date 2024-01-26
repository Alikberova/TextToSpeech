namespace BookToAudio.Core.Config;

public class EmailConfig
{
    public string EmailTo { get; set; } = string.Empty;
    public string EmailFrom {get; set; } = string.Empty;
    public string EmailFromPassword { get; set; } = string.Empty;
}
