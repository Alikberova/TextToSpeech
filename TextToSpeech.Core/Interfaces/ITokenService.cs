using TextToSpeech.Core.Entities;

namespace TextToSpeech.Core.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}