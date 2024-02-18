using System.Text.Json;
using System.Text.Json.Serialization;

namespace HexCore.GameMatch.States;


/// <summary>
/// State of the match
/// </summary>
public struct MatchState {
    public List<List<MatchLogEntryPart>> NewLogs { get; set; }
    public string Request { get; set; }
    public List<PlayerState> Players { get; set; }
    public MapState Map { get; set; }
    public MyDataState MyData { get; set; }
    public string CurPlayerID { get; set; }
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
        var result = JsonSerializer.Serialize(this, Common.JSON_SERIALIZATION_OPTIONS) ?? throw new Exception("Failed to serialize match");
        return result;
    }
    
    static public MatchState FromJson(string json) {
        var result = JsonSerializer.Deserialize<MatchState>(json, Common.JSON_SERIALIZATION_OPTIONS);
        return result;
    }
}