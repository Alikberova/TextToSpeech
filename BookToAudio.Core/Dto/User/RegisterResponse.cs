namespace BookToAudio.Core.Dto.User;

public sealed class RegisterResponse
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}
