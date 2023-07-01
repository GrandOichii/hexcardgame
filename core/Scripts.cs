using core.match;
using System.Reflection;
using System.Linq.Expressions;
using util;
using NLua;
using core.cards;
using core.tiles;

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
    /// Returns the tile at the specified coordinates
    /// </summary>
    /// <param name="coords">Coordinates in the form of a Lua table</param>
    /// <returns>Tile</returns>
    private Tile? TileAt(LuaTable coords) {
        long? iPos = coords[1] as long?;
        if (iPos is null) {
            throw new Exception("Invalid point coordinates");
        }
        long? jPos = coords[2] as long?;
        if (jPos is null) {
            throw new Exception("Invalid point coordinates");
        }

        // TODO explicit cast - bad
        var tile = _match.Map.Tiles[(int)iPos, (int)jPos];
        return tile;
    }

    /// <summary>
    /// Log wrapper for system log
    /// </summary>
    /// <param name="message">Log message</param>
    [LuaCommand]
    public void Log(string message) {
        _match.SystemLogger.Log("SCRIPTS", message);
    }

    /// <summary>
    /// Returns short string for player
    /// </summary>
    /// <param name="pID">Player ID</param>
    /// <returns>Short string</returns>
    [LuaCommand]
    public string PLayerShortStr(string pID) {
        var player = _match.PlayerWithID(pID);
        return player.ShortStr;
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
        return LuaUtility.CreateTable(_match.LState, result);
    }
    
    /// <summary>
    /// Sets the owner of the tiles to the specified player id
    /// </summary>
    /// <param name="points">List of points</param>
    /// <param name="pID">Player ID of the new owner of the tiles</param>
    [LuaCommand]
    public void TileOwnerSet(string pID, LuaTable points) {
        foreach (var pointRaw in points.Values) {
            var point = pointRaw as LuaTable;
            if (point is null) {
                throw new Exception("Invalid point arguments for TileOwnerSet function");
            }
            var tile = TileAt(point);
            if (tile is null) {
                // TODO? throw exception
                return;
            }


            // TODO for some reason pID is null, problem with setupScript
            var player = _match.PlayerWithID(pID);

            tile.Owner = player;
        }
    }


    [LuaCommand]
    public void CreateAndPutEntity(string pID, LuaTable point, string cID) {
        var player = _match.PlayerWithID(pID);
        var card = _match.CardMaster.Get(cID);
        var mCard = new MCard(_match, card, player);
        mCard.GoesToDiscard = false;

        var tile = TileAt(point);
        if (tile is null) {
            // TODO? throw exception
            return;
        }
        tile.Entity = mCard;
    }
}