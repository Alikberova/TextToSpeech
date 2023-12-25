using BookToAudio.Core.Config;
using BookToAudio.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BookToAudio.Core.Services;

public class AuthenticationService
{
    private readonly UserManager<User> _userManager;
    private readonly BtaUserManager _btaUserManager;
    private readonly Jwt _jwtConfig;

    public AuthenticationService(UserManager<User> userManager,
        BtaUserManager btaUserManager,
        IOptions<Jwt> jwtOptions)
    {
        _userManager = userManager;
        _btaUserManager = btaUserManager;
        _jwtConfig = jwtOptions.Value;
    }

    public async Task<string> Login(string userName, string password)
    {
        var user = await _btaUserManager.FindByNameAsync(userName);

        if (user is not null && await _userManager.CheckPasswordAsync(user, password))
        {
            var token = GenerateToken(user);
            return token;
        }

        return string.Empty;
    }

    private string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Symmetric.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

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
