using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TextToSpeech.Infra;
using TextToSpeech.Infra.Config;

namespace TextToSpeech.Api.Services;

public interface IJwtTokenService
{
    (DateTimeOffset expires, string jwtString) CreateGuestToken();
}

public sealed class JwtTokenService(IOptions<JwtConfig> jwtOptions) : IJwtTokenService
{
    private readonly JwtConfig _jwt = jwtOptions.Value;

    public (DateTimeOffset expires, string jwtString) CreateGuestToken()
    {
        var lifetimeMinutes = _jwt.GuestLifetimeMinutes > 0 ? _jwt.GuestLifetimeMinutes : 30;

        var subject = HostingEnvironment.IsTestMode()
            ? TestData.AudioOwnerId
            : $"guest:{Guid.NewGuid()}";

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

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: creds
        );

        var jwtString = new JwtSecurityTokenHandler().WriteToken(token);

        return (expires, jwtString);
    }
}
