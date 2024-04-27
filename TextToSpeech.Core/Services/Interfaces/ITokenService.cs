using TextToSpeech.Core.Entities;

namespace TextToSpeech.Core.Services.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}