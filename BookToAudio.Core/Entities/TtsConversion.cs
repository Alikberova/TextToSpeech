using System.ComponentModel.DataAnnotations;

namespace BookToAudio.Core.Entities;
public enum Status
{
    Pending,
    Processing,
    Completed,
    Failed

}

public enum Voice {
    Undefined,
    Alloy,
    Echo,
    Fable,
    Onyx,
    Nova,
    Shimmer
}
public class TtsConversion
{
    public Guid Id { get; set; }
    public Status Status { get; set; }
    [Required]
    public string? Model { get; set; }
    [Required]
    public Voice Voice { get; set; }
    public string ResponseFormat { get; set; } = string.Empty;
    public double Speed { get; set; }
}