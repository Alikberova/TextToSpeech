using BookToAudio.Core.Entities;
using BookToAudio.Core.Repositories;

namespace BookToAudio.Core.Services;

public class UserService(IUserRepository userRepository)
{
    private readonly IUserRepository _userRepository = userRepository;

    public User? GetUserById(Guid id)
    {
        return _userRepository.GetUserById(id);
    }

    public void AddUser(User user)
    {
        _userRepository.AddUser(user);
    }

    public User? UpdateUser(Guid id, User user)
    {
        return _userRepository.UpdateUser(id, user);
    }

    public bool DeleteUser(Guid id)
    {
        return _userRepository.DeleteUser(id);
    }

    public IEnumerable<User> GetUsers()
    {
        return _userRepository.GetUsers();
    }

    public bool UserExists(string email, string phone)
    {
        return _userRepository.UserExists(email, phone);
    }
}
