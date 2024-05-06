using ManagerBack.Dtos;

namespace ManagerBack.Services;

/// <summary>
/// User login result
/// </summary>
public class LoginResult {
    /// <summary>
    /// User token
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// User is admin flag
    /// </summary>
    public required bool IsAdmin { get; set; }
}

/// <summary>
/// User service
/// </summary>
public interface IUserService {
    /// <summary>
    /// Registers the user
    /// </summary>
    /// <param name="user">New user data</param>
    /// <returns>New user</returns>
    public Task<GetUserDto> Register(PostUserDto user);

    /// <summary>
    /// Attempts to login the user
    /// </summary>
    /// <param name="user">User data</param>
    /// <returns>User login result</returns>
    public Task<LoginResult> Login(PostUserDto user);
}