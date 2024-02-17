using System.Text.Json;
using System.Net.Sockets;

using System.Text.Json.Serialization;

using Core.GameMatch;

namespace Core.Manager;


public enum MatchTraceStatus
{
    WaitingForPlayers,
    InProgress,
    Crashed,
    Finished
}


public class MatchTrace
{
    [JsonIgnore]
    public Match Match { get; set; }
    [JsonIgnore]
    public Task Task { get; set; }
    [JsonIgnore]
    public TcpListener Listener { get; set; }

    [JsonPropertyName("id")]
    public string ID { get; set; }
    [JsonPropertyName("winner")]
    public string WinnerName { get; set; }
    [JsonPropertyName("status")]
    public MatchTraceStatus Status { get; set; } = MatchTraceStatus.WaitingForPlayers;
    [JsonPropertyName("url")]
    public string URL { get; set; }
}


public class ManagerMatchConfig {
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("config")]
    public MatchConfig Config { get; set; }
}

public class MatchCreationConfig {
    [JsonPropertyName("players")]
    public List<PlayerConfig> Players { get; set; }
    [JsonPropertyName("seed")]
    public string Seed { get; set; }
    [JsonPropertyName("config")]
    public MatchConfig Config { get; set; }
    [JsonPropertyName("batch")]
    public int Batch { get; set; }

    public string ToJson() {
        return JsonSerializer.Serialize(this);
    }
}

public class PlayerConfig {
    [JsonPropertyName("isBot")]
    public bool IsBot { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("decklist")]
    public string DeckList { get; set; }
}