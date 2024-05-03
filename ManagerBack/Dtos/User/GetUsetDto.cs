namespace ManagerBack.Models;

/// <summary>
/// Dto for requesting user information
/// </summary>
public class GetUserDto {
    /// <summary>
    /// User ID
    /// </summary>
    public required string Id { get; set; }
}