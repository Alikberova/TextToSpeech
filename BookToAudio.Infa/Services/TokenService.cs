using BookToAudio.Core.Config;
using BookToAudio.Core.Entities;
using BookToAudio.Core.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BookToAudio.Infa.Services;

public class TokenService : ITokenService
{
    private readonly JwtConfig _jwtConfig;

    public TokenService(IOptions<JwtConfig> jwtConfigOptions)
    {
        _jwtConfig = jwtConfigOptions.Value;
    }

    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Symmetric.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        //todo token expiration and refresh
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName!),
            // Add any additional claims you want to include in the token
        };

        var token = new JwtSecurityToken(
            issuer: _jwtConfig.Issuer,
            audience: _jwtConfig.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtConfig.ExpirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
