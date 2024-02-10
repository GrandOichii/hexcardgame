using ManagerBack.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ManagerBack.Controllers;


[ApiController]
[Route("/api/v1/auth")]
public class AuthController : ControllerBase {
    private readonly IUserService _userService;

    public AuthController(IUserService userService) {
        _userService = userService;
    }

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