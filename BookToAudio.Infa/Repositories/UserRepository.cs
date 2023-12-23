using BookToAudio.Core.Entities;
using BookToAudio.Core.Repositories;
using BookToAudio.Infa;

namespace BookToAudio.Infra.Repositories;

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    private readonly AppDbContext _dbContext = dbContext;

    public IEnumerable<User> GetUsers()
    {
        return _dbContext.Users.ToList();
    }

    public void AddUser(User newUser)
    {
        _dbContext.Users.Add(newUser);
        _dbContext.SaveChanges();
    }

    public bool DeleteUser(Guid id)
    {
        var userToDelete = _dbContext.Users.Find(id);

        if (userToDelete != null)
        {
            _dbContext.Users.Remove(userToDelete);
            _dbContext.SaveChanges();
            return true;
        }

        return false;
    }

    public User? UpdateUser(Guid id, User user)
    {
        var existingUser = _dbContext.Users.Find(id);

        if (existingUser == null)
        {
            return null;
        }
        existingUser.FirstName = user.FirstName;
        existingUser.LastName = user.LastName;
        existingUser.Email = user.Email;
        existingUser.Phone = user.Phone;

        _dbContext.SaveChanges();

        return existingUser;
    }

    public User? GetUserById(Guid id)
    {
        return _dbContext.Users.Find(id);
    }

    public bool UserExists(string email, string phone)
    {
        return _dbContext.Users.Any(u => u.Email == email || u.Phone == phone);
    }
}
