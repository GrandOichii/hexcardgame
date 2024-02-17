using Core.Cards;
using Core.GameMatch;
using NLua;
using Util;

namespace Core.GameMatch.Tiles;

// directions
//        0
//      -----   
// 5  /       \  1
//   /         \
//   \         /
// 4  \       /  2
//      -----     
//        3

/// <summary>
/// Tile object, represents a hexagonal tile
/// </summary>
public class Tile {
    public int IPos { get; }
    public int JPos { get; }
    public Player? Owner { get; set; }
    public MCard? Entity { get; set; }
    public bool HasGrave { get; set; } = false;
    public Tile(int iPos, int jPos)
    {
        IPos = iPos;
        JPos = jPos;
    }

    /// <summary>
    /// Creates and returns a Lua table with the info about the tile
    /// </summary>
    /// <param name="lState">Lua state</param>
    /// <returns>Lua table</returns>
    public LuaTable ToLuaTable(Lua lState) {
        var result = LuaUtility.CreateTable(lState);

        result["iPos"] = IPos;
        result["jPos"] = JPos;
        result["hasGrave"] = HasGrave;

        result["ownerID"] = null;
        if (Owner is not null) result["ownerID"] = Owner.ID;
        
        result["intity"] = null;
        if (Entity is not null) result["entity"] = Entity.Data;

        return result;
    }
}

