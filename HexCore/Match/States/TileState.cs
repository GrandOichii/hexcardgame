using System.Text.Json.Serialization;

namespace HexCore.GameMatch.States;


/// <summary>
/// State of a tile
/// </summary>
public struct TileState {
    public string OwnerID { get; set; }
    public bool HasGrave { get; set; }
    public MatchCardState? Entity { get; set; }
    public TileState(Tile tile) {
        OwnerID = "";
        if (tile.Owner is not null) OwnerID = tile.Owner.ID;

        HasGrave = tile.HasGrave;

        Entity = null;
        if (tile.Entity is not null) Entity = new MatchCardState(tile.Entity);
    }
}
