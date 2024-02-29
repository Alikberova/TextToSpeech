using BookToAudio.Core.Entities;
using BookToAudio.Core.Services.Interfaces;
using BookToAudio.Core.Dto.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LoginRequest = BookToAudio.Core.Dto.User.LoginRequest;
using BookToAudio.Api.Services;

namespace BookToAudio.Api.Controllers;

[Route("api/users")]
[ApiController]
public sealed class UserController : ControllerBase
{
    private readonly AuthenticationService _authentication;
    private readonly UserManager<User> _userManager;
    private readonly IBtaUserManager _btaUserManager;

    public UserController(AuthenticationService authentication,
        UserManager<User> userManager,
        IBtaUserManager btaUserManager)
    {
        _authentication = authentication;
        _userManager = userManager;
        _btaUserManager = btaUserManager;
    }

    // GET api/users
    [HttpGet]
    public IActionResult GetUsers()
    {
        return Ok(_userManager.Users);
    }

    // POST api/users/register
    [HttpPost("register")]
    public async Task<ActionResult> RegisterUser([FromBody] User user)
    {
        if (await _btaUserManager.UserExists(user.UserName!))
        {
            return Conflict("User with the same email or phone already exists.");
        }

        var createdUser = await _btaUserManager.CreateAsync(user);

        var response = new RegisterResponse
        {
            Id = createdUser.Id,
            Email = createdUser.Email,
            PhoneNumber = createdUser.PhoneNumber,
            UserName = createdUser.UserName!
        };

        return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, response);
    }

    // DELETE api/users/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        await _btaUserManager.DeleteAsync(id);

        return NoContent();
    }

    // PUT api/users/update
    [HttpPut("update")]
    public async Task<ActionResult> UpdateUser([FromBody] User user)
    {
        var updatedUser = await _btaUserManager.UpdateAsync(user);

        return Ok(updatedUser);
    }

    // GET api/users/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult> GetUserById(Guid id)
    {
        var user = await _btaUserManager.FindByIdAsync(id);

        if (user is null)
        {
            return NotFound("User not found.");
        }

        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginRequest request)
    {
        if (!await _btaUserManager.UserExists(request.Username))
        {
            return NotFound("User does not exist");
        }

        var token = await _authentication.Login(request.Username, request.Password);

        if (!string.IsNullOrWhiteSpace(token))
        {
            return Ok(new { Token = token });
        }

        return Unauthorized("Invalid username or password");
    }
}
