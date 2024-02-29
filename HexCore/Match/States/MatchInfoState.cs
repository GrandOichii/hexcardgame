using System.Text.Json;
using System.Text.Json.Serialization;

namespace HexCore.GameMatch.States;


/// <summary>
/// Match info state, used for sending to players for UI setup
/// </summary>
public struct MatchInfoState {
    public string MatchId { get; set; }
    public string? MyID { get; set; }
    public int? MyI { get; set; }
    public int PlayerCount { get; set; }

    public MatchInfoState(Player player, Match match) : this(match) {
        MyID = player.ID;
        MyI = match.Players.IndexOf(player);
    }

    public MatchInfoState(Match match) {
        PlayerCount = match.Players.Count;
        MatchId = match.ID;
    }

    /// <summary>
    /// Converts the match info to JSON
    /// </summary>
    /// <returns>JSON string</returns>    
    public string ToJson() {
        var result = JsonSerializer.Serialize(this, Common.JSON_SERIALIZATION_OPTIONS) ?? throw new Exception("Failed to serialize match");
        return result;
    }

    /// <summary>
    /// Parses JSON to a MatchInfoState object
    /// </summary>
    /// <param name="json">JSON text</param>
    /// <returns>Parsed object</returns>
    static public MatchInfoState FromJson(string json) {
        var result = JsonSerializer.Deserialize<MatchInfoState>(json, Common.JSON_SERIALIZATION_OPTIONS);
        return result;
    }
}
