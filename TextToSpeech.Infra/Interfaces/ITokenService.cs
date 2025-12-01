using TextToSpeech.Infra.Models;

namespace TextToSpeech.Infra.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}