using System.Text;
using NLua;

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
/// Empty logger class, logged information is not saved anywhere
/// </summary>
public class EmptyLogger : Logger
{
    public override void Log(string prefix, string text)
    {
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
        string data = prefix + ": " + text + "\n";
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


/// <summary>
/// Static class of Utiity functions for Lua
/// </summary>
static class LuaUtil {
    /// <summary>
    /// Creates a new Lua table
    /// </summary>
    /// <param name="lState">Lua state</param>
    /// <returns>New table</returns>
    static public LuaTable CreateTable(Lua lState) {
        lState.NewTable("_table");
        return lState.GetTable("_table");
    }

    /// <summary>
    /// Creates a new Lua array
    /// </summary>
    /// <param name="lState">Lua state</param>
    /// <param name="args">The array</param>
    /// <returns>Lua array</returns>
    static public LuaTable CreateTable<T>(Lua lState, List<T> args) {
        var result = CreateTable(lState);
        for (int i = 0; i < args.Count; i++)
            result[i+1] = args[i];
        return result;
    }
}