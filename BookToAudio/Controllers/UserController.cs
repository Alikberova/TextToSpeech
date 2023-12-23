using BookToAudio.Core.Entities;
using BookToAudio.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookToAudio.Controllers;

[Route("api/users")]
[ApiController]
public class UserController(UserService userService) : ControllerBase
{
    private readonly UserService _userService = userService;

    // GET api/users
    [HttpGet]
    public IActionResult GetUsers()
    {
        return Ok(_userService);
    }

    // POST api/users/register
    [HttpPost("register")]
    public IActionResult RegisterUser([FromBody] User user)
    {
        if (UserExists(user.Email, user.Phone))
        {
            return Conflict("User with the same email or phone already exists.");
        }

        user.Id = Guid.NewGuid();
        _userService.AddUser(user);

        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
    }

    // DELETE api/users/{id}
    [HttpDelete("{id}")]
    public IActionResult DeleteUser(Guid id)
    {
        if (!_userService.DeleteUser(id))
        {
            return NotFound("User not found.");
        }

        return NoContent();
    }

    // PUT api/users/{id}
    [HttpPut("{id}")]
    public IActionResult UpdateUser(Guid id, [FromBody] User user)
    {
        var updatedUser = _userService.UpdateUser(id, user);

        if (updatedUser is  null)
        {
            return NotFound("User not found.");
        }

        return Ok(updatedUser);
    }

    // GET api/users/{id}
    [HttpGet("{id}")]
    public IActionResult GetUserById(Guid id)
    {
        var user = _userService.GetUserById(id);

        if (user is null)
        {
            return NotFound("User not found.");
        }

        return Ok(user);
    }

    private bool UserExists(string email, string phone)
    {
        return _userService.UserExists(email, phone);
    }
}
