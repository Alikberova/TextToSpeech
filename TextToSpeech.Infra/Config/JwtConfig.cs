namespace TextToSpeech.Infra.Config;

public sealed class JwtConfig
{
    public Symmetric Symmetric { get; init; } = new();
    public Asymmetric Asymmetric { get; init; } = new();
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int ExpirationMinutes { get; init; }
}

public sealed class Asymmetric
{
    public string PrivateKey { get; init; } = string.Empty;
    public string PublicKey { get; init; } = string.Empty;
}

public sealed class Symmetric
{
    public string Key { get; init; } = string.Empty;
}
