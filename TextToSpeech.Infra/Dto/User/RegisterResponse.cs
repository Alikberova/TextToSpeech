namespace TextToSpeech.Infra.Dto.User;

public sealed record RegisterResponse
{
    public string Id { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
}
