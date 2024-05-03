namespace ManagerBack.Services;

/// <summary>
/// Connection checker
/// </summary>
public interface IConnectionChecker {
    /// <summary>
    /// Reads a message
    /// </summary>
    /// <returns>The read message</returns>
    public Task<string> Read();

    /// <summary>
    /// Writes the message
    /// </summary>
    /// <param name="msg">Message</param>
    public Task Write(string msg);

    /// <summary>
    /// Check, whether the connection is valid
    /// </summary>
    /// <returns>True is connection is valid, else false</returns>
    public Task<bool> Check();
}
