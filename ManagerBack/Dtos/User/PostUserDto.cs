namespace ManagerBack.Dtos;

/// <summary>
/// Dto for posting user information
/// </summary>
public class PostUserDto {
    /// <summary>
    /// Username
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// Password
    /// </summary>
    public required string Password { get; set; }
}