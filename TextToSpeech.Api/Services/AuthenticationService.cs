using TextToSpeech.Core.Entities;
using TextToSpeech.Core.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace TextToSpeech.Api.Services;

public sealed class AuthenticationService
{
    private readonly UserManager<User> _userManager;
    private readonly IBtaUserManager _btaUserManager;
    private readonly ITokenService _tokenService;

    public AuthenticationService(UserManager<User> userManager,
        IBtaUserManager btaUserManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _btaUserManager = btaUserManager;
        _tokenService = tokenService;
    }

    internal async Task<string> Login(string userName, string password)
    {
        var user = await _btaUserManager.FindByNameAsync(userName);

        if (user is not null && await _userManager.CheckPasswordAsync(user, password))
        {
            return _tokenService.GenerateToken(user);
        }

        return string.Empty;
    }
}
