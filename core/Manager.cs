using System.Text.Json;
using System.Net.Sockets;

using System.Text.Json.Serialization;

using core.match;

namespace core.manager;


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
    [JsonPropertyName("status")]
    public MatchTraceStatus Status { get; set; } = MatchTraceStatus.WaitingForPlayers;
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