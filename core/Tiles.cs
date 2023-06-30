using core.match;

namespace core.tiles;

/// <summary>
/// Tile object, represents a hexagonal tile
/// </summary>
public class Tile {
    public int IPos { get; }
    public int JPos { get; }

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

    public Map(Match match, int width, int height) {
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
        var result = new Map(match, 1, 1);

        return result;
    }
}