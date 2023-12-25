using BookToAudio.Core.Entities;
using BookToAudio.Core.Repositories;
using Microsoft.AspNetCore.Identity;

namespace BookToAudio.Core.Services;

public class UserService(IUserRepository userRepository, UserManager<User> userManager)
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly UserManager<User> _userManager = userManager;

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await _userRepository.GetUserByIdAsync(id);
    }

    public async Task AddUserAsync(User user)
    {
        await _userRepository.AddUserAsync(user);
    }

    public async Task<User> UpdateUserAsync(Guid id, User user)
    {
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            throw new Exception(string.Join('\n', result.Errors));
        }

        return await _userRepository.UpdateUserAsync(id, user);
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        return await _userRepository.DeleteUserAsync(id);
    }

    public IEnumerable<User> GetUsers()
    {
        return _userRepository.GetUsers();
    }

    public async Task<bool> UserExistsAsync(string? userName)
    {
        return await _userRepository.UserExistsAsync(userName);
    }

    public User? GetUserByUsername(string username)
    {
        return _userRepository.GetUserByUsername(username);
    }
}
