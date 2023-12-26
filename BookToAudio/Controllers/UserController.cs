using BookToAudio.Core.Entities;
using BookToAudio.Core.Services;
using BookToAudio.Infa.Dto;
using BookToAudio.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LoginRequest = BookToAudio.Infa.Dto.LoginRequest;

namespace BookToAudio.Controllers;

[Route("api/users")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly AuthenticationService _authentication;
    private readonly UserManager<User> _userManager;
    private readonly BtaUserManager _btaUserManager;

    public UserController(AuthenticationService authentication, UserManager<User> userManager, BtaUserManager btaUserManager)
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
        if (await _btaUserManager.UserExists(user.UserName))
        {
            return Conflict("User with the same email or phone already exists.");
        }

        await _btaUserManager.CreateAsync(user);

        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
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

    [HttpPost("userExists")]
    public async Task<ActionResult> UserExists([FromBody] UserCheckRequest request)
    {
        return Ok(await _btaUserManager.UserExists(request.UserName));
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await _authentication.Login(request.Username, request.Password);

        if (!string.IsNullOrWhiteSpace(token))
        {
            return Ok(new { Token = token });
        }

        return Unauthorized("Invalid username or password");
    }
}
