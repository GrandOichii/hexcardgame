using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.GameMatch.States;


/// <summary>
/// Match info state, used for sending to players for UI setup
/// </summary>
public struct MatchInfoState {
    [JsonPropertyName("myID")]
    public string MyID { get; set; }
    [JsonPropertyName("myI")]
    public int MyI { get; set; }
    [JsonPropertyName("playerCount")]
    public int PlayerCount { get; set; }

    public MatchInfoState(Player player, Match match) {
        MyID = player.ID;
        MyI = match.Players.IndexOf(player);
        PlayerCount = match.Players.Count;
    }

    /// <summary>
    /// Converts the match info to JSON
    /// </summary>
    /// <returns>JSON string</returns>    
    public string ToJson() {
        var result = JsonSerializer.Serialize(this) ?? throw new Exception("Failed to serialize match");
        return result;
    }

    /// <summary>
    /// Parses JSON to a MatchInfoState object
    /// </summary>
    /// <param name="json">JSON text</param>
    /// <returns>Parsed object</returns>
    static public MatchInfoState FromJson(string json) {
        var result = JsonSerializer.Deserialize<MatchInfoState>(json);
        return result;
    }
}
