using System.Text.Json.Serialization;

namespace ManagerBack.Services;

public enum BotType {
    RANDOM,
    SMART
}

public class BotConfig {
    public required string StrDeck { get; set; }
    public required string Name { get; set; }
    public required BotType BotType { get; set; }
}

public class PlayerConfig {
    public required BotConfig? BotConfig { get; set; }
}

public class MatchProcessConfig {
    // TODO? change to matchConfigId when will implement ConfigController
    [JsonPropertyName("mConfig")]
    public required string MatchConfigId { get; set; }

    public required PlayerConfig P1Config { get; set; }
    public required PlayerConfig P2Config { get; set; }
}