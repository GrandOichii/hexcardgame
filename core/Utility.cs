using System.Text;

namespace util;

/// <summary>
/// Abstract logger class, logs info with a prefix
/// </summary>
abstract public class Logger {
    /// <summary>
    /// Logs the info in the format of [prefix]: [text]
    /// </summary>
    /// <param name="prefix">Prefix of the information</param>
    /// <param name="text">Information</param>
    abstract public void Log(string prefix, string text);
}


/// <summary>
/// Basic console logger
/// </summary>
public class ConsoleLogger : Logger
{
    public override void Log(string prefix, string text)
    {
        System.Console.WriteLine(prefix + ": " + text);
    }
}


/// <summary>
/// Basic file logger
/// </summary>
public class FileLogger : Logger
{
    public FileStream Stream { get; }
    public FileLogger(string path) : base() {
        Stream = File.OpenWrite(path);
    }

    // TODO not tested
    public override void Log(string prefix, string text)
    {
        string data = prefix + ": " + text;
        byte[] info = new UTF8Encoding(true).GetBytes(data);
        Stream.Write(info, 0, info.Length);
        Stream.Flush();
    }
}


/// <summary>
/// Abstract class for creating unique ids
/// </summary>
abstract public class IDCreator {
    /// <summary>
    /// Generates a new unique ID
    /// </summary>
    /// <returns>New ID</returns>
    abstract public string Next();
}


/// <summary>
/// Basic ID creator, gives out a string format of a number
/// </summary>
public class BasicIDCreator : IDCreator {
    private int _count = 0;
    public override string Next()
    {
        return (++_count).ToString();
    }
}