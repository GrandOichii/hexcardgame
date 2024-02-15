using System.Text.Json.Serialization;

namespace ManagerBack.Services;

public class PlayerConfig {
    public required bool IsBot { get; set; }
}

public class MatchProcessConfig {
    [JsonPropertyName("mConfig")]
    public required MatchConfig MatchConfig { get; set; }

    public required PlayerConfig P1Config { get; set; }
    public required PlayerConfig P2Config { get; set; }
}