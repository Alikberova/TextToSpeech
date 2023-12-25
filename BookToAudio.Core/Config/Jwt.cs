namespace BookToAudio.Core.Config;

public class Jwt
{
    public Symmetric Symmetric { get; set; } = new();
    public Asymmetric Asymmetric { get; set; } = new();
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; }
}

public class Asymmetric
{
    public string PrivateKey { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
}

public class Symmetric
{
    public string Key { get; init; } = string.Empty;
}
