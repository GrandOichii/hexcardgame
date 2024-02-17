using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.GameMatch.States;


/// <summary>
/// State of the match
/// </summary>
public struct MatchState {
    [JsonPropertyName("newLogs")]
    public List<List<MatchLogEntryPart>> NewLogs { get; set; }
    [JsonPropertyName("request")]
    public string Request { get; set; }
    [JsonPropertyName("players")]
    public List<PlayerState> Players { get; set; }
    [JsonPropertyName("map")]
    public MapState Map { get; set; }
    [JsonPropertyName("myData")]
    public MyDataState MyData { get; set; }
    [JsonPropertyName("curPlayerID")]
    public string CurPlayerID { get; set; }
    [JsonPropertyName("args")]
    public List<string> Args { get; set; }

    public MatchState(Match match, Player player, string request, List<string>? args=null) {
        if (args == null) args = new();

        NewLogs = player.NewLogs;
        Args = args;
        player.NewLogs = new();

        CurPlayerID = match.CurrentPlayer.ID;
        Request = request;

        MyData = new MyDataState(player);
        
        Players = new();
        foreach (var p in match.Players)
            Players.Add(new PlayerState(p));
        
        Map = new MapState(match.Map);
    }

    /// <summary>
    /// Converts the match state to JSON
    /// </summary>
    /// <returns>JSON string</returns>
    public string ToJson() {
        var result = JsonSerializer.Serialize(this) ?? throw new Exception("Failed to serialize match");
        return result;
    }
    
    static public MatchState FromJson(string json) {
        var result = JsonSerializer.Deserialize<MatchState>(json);
        return result;
    }
}