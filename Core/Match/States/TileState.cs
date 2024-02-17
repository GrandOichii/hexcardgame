using System.Text.Json.Serialization;

namespace Core.GameMatch.States;


/// <summary>
/// State of a tile
/// </summary>
public struct TileState {
    [JsonPropertyName("ownerID")]
    public string OwnerID { get; set; }
    [JsonPropertyName("hasGrave")]
    public bool HasGrave { get; set; }
    [JsonPropertyName("entity")]
    public MCardState? Entity { get; set; }
    public TileState(Tile tile) {
        OwnerID = "";
        if (tile.Owner is not null) OwnerID = tile.Owner.ID;

        HasGrave = tile.HasGrave;

        Entity = null;
        if (tile.Entity is not null) Entity = new MCardState(tile.Entity);
    }
}
