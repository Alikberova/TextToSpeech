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

    public async Task<bool> UserExists(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return false;
        }

        return await _userManager.FindByNameAsync(userName) is not null;
    }

    public async Task<User> CreateAsync(User user)
    {
        var result = await _userManager.CreateAsync(user, user.Password);

        CheckErrors(result);

        return (await _userManager.FindByIdAsync(user.Id.ToString()))!;
    }

    public async Task<User> UpdateAsync(User user)
    {
        var result = await _userManager.UpdateAsync(user);

        CheckErrors(result);

        return (await _userManager.FindByIdAsync(user.Id))!;
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString()) ??
            throw new InvalidOperationException("User not found.");

        var result = await _userManager.DeleteAsync(user);

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

        throw new IdentityOperationException(errors);
    }
}

public class IdentityOperationException(string message) : Exception(message)
{
}
