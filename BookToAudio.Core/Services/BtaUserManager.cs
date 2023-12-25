using BookToAudio.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace BookToAudio.Core.Services;

public class BtaUserManager
{
    private readonly UserManager<User> _userManager;

    public BtaUserManager(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> UserExists(string? userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return false;
        }

        return await FindByNameAsync(userName) is not null;
    }

    public async Task CreateAsync(User user)
    {
        var result = await _userManager.CreateAsync(user);
        CheckErrors(result);
        //todo password validation angular
        //at least one uppercase ('A'-'Z'), one special character, at least 6 characters

        var pasResult = await _userManager.AddPasswordAsync(user, user.Password);
        CheckErrors(pasResult);
    }

    public async Task<User> UpdateAsync(Guid id, User user)
    {
        var result = await _userManager.UpdateAsync(user);

        CheckErrors(result);

        return user;
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await FindByIdAsync(id);

        var result = await _userManager.DeleteAsync(user!);

        CheckErrors(result);
    }

    public async Task<User?> FindByIdAsync(Guid id)
    {
        return await _userManager.FindByIdAsync(id.ToString());
    }

    public async Task<User?> FindByNameAsync(string userName)
    {
        return await _userManager.FindByNameAsync(userName);
    }

    private static void CheckErrors(IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join('\n', result.Errors.Select(e => e.Description));
        throw new Exception(errors);
    }
}
