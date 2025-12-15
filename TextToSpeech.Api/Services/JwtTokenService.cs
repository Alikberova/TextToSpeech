using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TextToSpeech.Api.Services;

public interface IJwtTokenService
{
    (DateTimeOffset expires, string jwtString) CreateGuestToken();
}

public sealed class JwtTokenService(IConfiguration config) : IJwtTokenService
{
    public (DateTimeOffset expires, string jwtString) CreateGuestToken()
    {
        var jwt = config.GetSection("Jwt");
        var issuer = jwt["Issuer"]!;
        var audience = jwt["Audience"]!;
        var signingKey = jwt["SigningKey"]!;
        var lifetimeMinutes = int.TryParse(jwt["GuestLifetimeMinutes"], out var m) ? m : 30;

        var guestId = Guid.NewGuid();
        var subject = $"guest:{guestId}";

        var now = DateTimeOffset.UtcNow;
        var expires = now.AddMinutes(lifetimeMinutes);
        var claims = new List<Claim>
        {
            // sub claim
            new(JwtRegisteredClaimNames.Sub, subject),

            // iat claim (numeric date, seconds)
            new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),

            // token identifier
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: creds
        );

        var jwtString = new JwtSecurityTokenHandler().WriteToken(token);

        return (expires, jwtString);
    }
}
