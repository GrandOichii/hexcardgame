using System.Text.Json.Serialization;

namespace Core.GameMatch.States;

/// <summary>
/// State of the map
/// </summary>
public struct MapState {
    [JsonPropertyName("tiles")]
    public List<List<TileState?>> Tiles { get; set; }

    public MapState(Map map) {
        Tiles = new ();
        for (int i = 0; i < map.Height; i++) {
            var l = new List<TileState?>();
            for (int j = 0; j < map.Width; j++) {
                var tile = map.Tiles[i, j];
                if (tile is null) {
                    l.Add(null);
                    continue;
                }

                l.Add(new TileState(tile));
            }
            Tiles.Add(l);
        }
    }
}
