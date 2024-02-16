using System.Text.Json.Serialization;

namespace ManagerBack.Services;

public class BotConfig {
    public required string StrDeck { get; set; }
    public required string Name { get; set; }
}

public class PlayerConfig {
    // TODO replace with bot config class
    public required BotConfig? BotConfig { get; set; }
}

public class MatchProcessConfig {
    [JsonPropertyName("mConfig")]
    public required MatchConfig MatchConfig { get; set; }

    public required PlayerConfig P1Config { get; set; }
    public required PlayerConfig P2Config { get; set; }
}