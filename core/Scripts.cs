using core.match;
using System.Reflection;
using System.Linq.Expressions;
using util;
using NLua;

namespace core.scripts;


/// <summary>
/// Marks the method as a Lua function
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
internal class LuaCommand : Attribute {}

/// <summary>
/// Script master of the match, creates all the utility function
/// </summary>
public class ScriptMaster {
    private Match _match;
    public ScriptMaster(Match match) {
        _match = match;

        // load all methods into the Lua stat
        var type = typeof(ScriptMaster);
        foreach (var method in type.GetMethods())
        {
            if (method.GetCustomAttribute(typeof(LuaCommand)) is not null)
            {
                _match.LState[method.Name] = method.CreateDelegate(Expression.GetDelegateType(
                    (from parameter in method.GetParameters() select parameter.ParameterType)
                    .Concat(new[] { method.ReturnType })
                .ToArray()), this);
            }
        }
    }


    /// <summary>
    /// Returns a Lua array with all the player IDs
    /// </summary>
    /// <returns>Lua array of player IDs</returns>
    [LuaCommand]
    public LuaTable GetPlayerIDs() {
        var result = new List<string>();
        foreach (var player in _match.Players)
            result.Add(player.ID);
        return LuaUtil.CreateTable(_match.LState, result);
    }

    
    /// <summary>
    /// Sets the owner of the tiles to the specified player id
    /// </summary>
    /// <param name="points">List of points</param>
    /// <param name="pID">Player ID of the new owner of the tiles</param>
    [LuaCommand]
    public void TileOwnerSet(LuaTable points, string pID) {
        foreach (var pointRaw in points.Values) {
            var point = pointRaw as LuaTable;
            if (point is null) {
                throw new Exception("Invalid point arguments for TileOwnerSet function");
            }
            long? iPos = point[1] as long?;
            if (iPos is null) {
                throw new Exception("Invalid point arguments for TileOwnerSet function");
            }
            long? jPos = point[2] as long?;
            if (jPos is null) {
                throw new Exception("Invalid point arguments for TileOwnerSet function");
            }

            // TODO
        }
    }


    [LuaCommand]
    public void CreateAndPutEntity(string pID, LuaTable point, string cID) {
        // TODO
    }
}