namespace Util.Logging;

/// <summary>
/// Empty logger class, logged information is not saved anywhere
/// </summary>
public class EmptyLogger : ILogger
{
    public void Log(string prefix, string text)
    {
    }
}
