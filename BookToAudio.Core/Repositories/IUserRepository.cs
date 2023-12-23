using BookToAudio.Core.Entities;

namespace BookToAudio.Core.Repositories;

public interface IUserRepository
{
    User? GetUserById(Guid id);
    void AddUser(User user);
    User? UpdateUser(Guid id, User user);
    bool DeleteUser(Guid id);
    IEnumerable<User> GetUsers();
    bool UserExists(string email, string phone);
}
