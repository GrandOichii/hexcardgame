namespace Util.Logging;

using System.Text;

/// <summary>
/// Basic file logger
/// </summary>
public class FileLogger : ILogger
{
    public FileStream Stream { get; }
    public FileLogger(string path) : base() {
        Stream = File.OpenWrite(path);
    }

    public void Log(string prefix, string text)
    {
        string data = prefix + ": " + text + "\n";
        byte[] info = new UTF8Encoding(true).GetBytes(data);
        Stream.Write(info, 0, info.Length);
        Stream.Flush();
    }
}

