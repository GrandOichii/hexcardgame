namespace Util.Logging;

/// <summary>
/// Basic console logger
/// </summary>
public class ConsoleLogger : ILogger
{
    public void Log(string prefix, string text)
    {
        System.Console.WriteLine(prefix + ": " + text);
    }
}
