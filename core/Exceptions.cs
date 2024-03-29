namespace Core.Exceptions;

using NLua;

[System.Serializable]
public class GLuaTableException : Exception
{
    public GLuaTableException(LuaTable table, string message) : base(message) { }
}
