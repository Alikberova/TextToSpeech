using Microsoft.AspNetCore.Identity;

namespace TextToSpeech.Infra.Models;

public sealed class User : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
