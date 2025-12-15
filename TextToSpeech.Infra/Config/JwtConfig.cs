namespace TextToSpeech.Infra.Config;

public sealed class JwtConfig
{
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string SigningKey { get; init; } = string.Empty;
    public int GuestLifetimeMinutes { get; init; }
}
