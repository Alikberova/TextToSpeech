using BookToAudio.Core.Entities;

namespace BookToAudio.Core.Services.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}