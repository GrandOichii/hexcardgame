namespace Util.Logging;

/// <summary>
/// Abstract logger class, logs info with a prefix
/// </summary>
public interface ILogger {
    /// <summary>
    /// Logs the info in the format of [prefix]: [text]
    /// </summary>
    /// <param name="prefix">Prefix of the information</param>
    /// <param name="text">Information</param>
    public void Log(string prefix, string text);
}
