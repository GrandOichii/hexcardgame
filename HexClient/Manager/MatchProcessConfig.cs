using System.Text.Json.Serialization;

namespace HexClient.Manager;

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
    #nullable enable
    public required BotConfig? BotConfig { get; set; }
    #nullable disable
}

public class MatchProcessConfig {
    [JsonPropertyName("mConfig")]
    public required string MatchConfigId { get; set; }

    public required bool CanWatch { get; set; }

    public required PlayerConfig P1Config { get; set; }
    public required PlayerConfig P2Config { get; set; }
}