using core.match;
using core.players;

namespace core.tiles;

/// <summary>
/// Tile object, represents a hexagonal tile
/// </summary>
public class Tile {
    public int IPos { get; }
    public int JPos { get; }
    public Player? Owner { get; set; }

    public Tile(int iPos, int jPos)
    {
        IPos = iPos;
        JPos = jPos;
    }
}


/// <summary>
/// Map object, consists of multiple hexagonal tiles
/// </summary>
public class Map {
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
}