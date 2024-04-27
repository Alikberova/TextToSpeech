using TextToSpeech.Core.Entities;
using TextToSpeech.Core.Repositories;
using TextToSpeech.Infra;
using Microsoft.EntityFrameworkCore;

namespace TextToSpeech.Infra.Repositories;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public IEnumerable<User> GetUsers()
    {
        return _dbContext.Users;
    }

    public async Task AddUserAsync(User newUser)
    {
        await _dbContext.Users.AddAsync(newUser);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var userToDelete = _dbContext.Users.Find(id);

        if (userToDelete != null)
        {
            _dbContext.Users.Remove(userToDelete);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<User> UpdateUserAsync(Guid id, User user)
    {
        var existingUser = await _dbContext.Users.FindAsync(id);

        existingUser!.FirstName = user.FirstName;
        existingUser.LastName = user.LastName;
        existingUser.Email = user.Email;
        existingUser.PhoneNumber = user.PhoneNumber;
        existingUser.UserName = user.UserName;

        await _dbContext.SaveChangesAsync();

        return existingUser;
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await _dbContext.Users.SingleOrDefaultAsync(u => u.Id == id.ToString());
    }

    public async Task<bool> UserExistsAsync(string? userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return false;
        }

        return await _dbContext.Users.AnyAsync(u => u.UserName == userName);
    }

    public User? GetUserByUsername(string username)
    {
        return _dbContext.Users.SingleOrDefault(u => u.UserName == username);
    }
}
