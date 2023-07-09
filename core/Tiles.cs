using core.cards;
using core.match;
using core.players;
using NLua;
using util;

namespace core.tiles;

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


/// <summary>
/// Map object, consists of multiple hexagonal tiles
/// </summary>
public class Map {
    static public readonly int[][][] DIR_ARR = new int[][][]{

        new int[][] {
            new int[]{-2, 0},
            new int[]{-1, +1},
            new int[]{+1, +1},
            new int[]{+2, 0},
            new int[]{+1, 0},
            new int[]{-1, 0}
        },
        new int[][] {
            new int[]{-2, 0},
            new int[]{-1, 0},
            new int[]{+1, 0},
            new int[]{+2, 0},
            new int[]{+1, -1},
            new int[]{-1, -1},
        }
    };
    
    public Match Match { get; }
    public Tile?[,] Tiles { get; }
    public int Width { get; }
    public int Height { get; }

    public Map(Match match, int width, int height) {
        Width = width;
        Height = height;

        Match = match;
        Tiles = new Tile?[height, width];
    }

    /// <summary>
    /// Constructs the map using the match configuration object
    /// </summary>
    /// <param name="config">Match configuration</param>
    /// <returns>Constructed map</returns>
    static public Map FromConfig(Match match, MatchConfig config) {
        // TODO
        var map = config.Map;
        var setupScript = config.SetupScript;
        var height = map.Count;
        var width = map[0].Count;
        var result = new Map(match, width, height);
        for (int i = 0; i < height; i++) {
            var row = map[i];
            for (int j = 0; j < row.Count; j++)
                if (row[j] != 0)
                    result.Tiles[i, j] = new Tile(i, j);
        }

        return result;
    }

    /// <summary>
    /// Returns the tile at the specified string location
    /// </summary>
    /// <param name="point">Tile location, should be in the format of [i]:[j]</param>
    /// <returns></returns>
    public Tile? TileAt(string point) {
        var p = PointSplit(point);
        return Tiles[p[0], p[1]];
    }

    /// <summary>
    /// Splits the point into an array of integers
    /// </summary>
    /// <param name="point">Point string, should be in the format of [i]:[j]</param>
    /// <returns>An array of i and j</returns>
    private int[] PointSplit(string point) {
        var split = point.Split(".");
        if (split.Length != 2) {
            throw new Exception("Can't split point string " + point);
        }
        var i = int.Parse(split[0]);
        var j = int.Parse(split[1]);
        return new int[] {i, j};
    }

    public Tile? GetNeighbor(int iLoc, int jLoc, int dir) {
        var dirs = DIR_ARR[iLoc % 2];
        var diff = dirs[dir];
        var newI = iLoc + diff[0];
        var newJ = jLoc + diff[1];
        if (newI < 0 || newJ < 0) return null;
        if (newI >= Tiles.GetLength(0)) return null;
        if (newJ >= Tiles.GetLength(1)) return null;
        return Tiles[newI, newJ];
    }

}