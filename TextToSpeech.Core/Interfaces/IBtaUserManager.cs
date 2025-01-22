using TextToSpeech.Core.Entities;

namespace TextToSpeech.Core.Interfaces;

public interface IBtaUserManager
{
    Task<User> CreateAsync(User user);
    Task DeleteAsync(Guid id);
    Task<User?> FindByIdAsync(Guid id);
    Task<User?> FindByNameAsync(string userName);
    Task<User> UpdateAsync(User user);
    Task<bool> UserExists(string userName);
}