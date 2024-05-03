using ManagerBack.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ManagerBack.Controllers;

/// <summary>
/// Api controller for accessing authentication
/// </summary>
[ApiController]
[Route("/api/v1/auth")]
public class AuthController : ControllerBase {
    /// <summary>
    /// User service
    /// </summary>
    private readonly IUserService _userService;

    public AuthController(IUserService userService) {
        _userService = userService;
    }

    /// <summary>
    /// Endpoint for registering a new user
    /// </summary>
    /// <param name="user">New user data</param>
    /// <returns>Response object with the new user information as data</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] PostUserDto user) {
        try {

            var result = await _userService.Register(user);
            return Ok(result);
        
        } catch (UsernameTakenException e) {
            return Conflict(e.Message);
        } catch (InvalidRegisterCredentialsException e) {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Endpoint for logging in as a user
    /// </summary>
    /// <param name="user">User data</param>
    /// <returns>Response object with a jwt token as data</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] PostUserDto user) {
        try {

            var result = await _userService.Login(user);
            return Ok(result);
        
        } catch (InvalidLoginCredentialsException e) {
            return BadRequest(e.Message);
        }
    }
}