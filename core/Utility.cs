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
static class LuaUtility {
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

    /// <summary>
    /// Fetches global function from Lua state
    /// </summary>
    /// <param name="lState">Lua state</param>
    /// <param name="fName">Function name</param>
    /// <returns>The function</returns>
    static public LuaFunction GetGlobalF(Lua lState, string fName) {
        var f = lState[fName] as LuaFunction;
        if (f is null) throw new Exception("Failed to get function " + fName + " from glabal Lua state");
        return f;
    } 

    /// <summary>
    /// Checks whether can index the array, if not throws exception
    /// </summary>
    /// <param name="returned">The array</param>
    /// <param name="index">The index</param>
    static void CheckIndex(object[] returned, int index) {
        if (index < returned.Length) return;

        throw new Exception("Can't access return value with index " + index + ": total amount of returned values is " + returned.Length);
    }


    /// <summary>
    /// Selects the return value at specified index and returns it as an object of the specified type
    /// </summary>
    /// <typeparam name="T">Type of the return value</typeparam>
    static public T GetReturnAs<T>(object[] returned, int index=0) where T : class {
        CheckIndex(returned, index);
        var result = returned[index] as T;
        if (result is null) throw new Exception("Return value in index " + index + " is not a table");
        return result;
    }
}


/// <summary>
/// General utility class
/// </summary>
static class Utility {
    /// <summary>
    /// Returns the shuffled list
    /// </summary>
    /// <param name="list">List</param>
    /// <param name="rnd">Random number generator</param>
    /// <typeparam name="T">Type of contained value</typeparam>
    /// <returns>The shuffled list</returns>
    static public List<T> Shuffled<T>(List<T> list, Random rnd) {
        return list.OrderBy(a => rnd.Next()).ToList();
    }
        

}