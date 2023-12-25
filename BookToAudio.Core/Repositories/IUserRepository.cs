using BookToAudio.Core.Entities;

namespace BookToAudio.Core.Repositories;

public interface IUserRepository
{
    Task AddUserAsync(User newUser);
    Task<bool> DeleteUserAsync(Guid id);
    Task<User?> GetUserByIdAsync(Guid id);
    User? GetUserByUsername(string username);
    IEnumerable<User> GetUsers();
    Task<User> UpdateUserAsync(Guid id, User user);
    Task<bool> UserExistsAsync(string? userName);
}
