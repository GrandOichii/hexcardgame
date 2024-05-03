namespace ManagerBack.Repositories;

/// <summary>
/// User repository
/// </summary>
public interface IUserRepository {
    /// <summary>
    /// Add a new user to the repository
    /// </summary>
    /// <param name="user">New user data</param>
    public Task Add(User user);

    /// <summary>
    /// Fetches the user by their username
    /// </summary>
    /// <param name="username">Username</param>
    /// <returns>User if exists, else null</returns>
    public Task<User?> ByUsername(string username);

    /// <summary>
    /// Checks, whether a user with the specified ID exists
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>True if user exists, else false</returns>
    public Task<bool> CheckId(string id);
}