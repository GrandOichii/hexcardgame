using System.Text.Json.Serialization;

namespace ManagerBack.Services;

/// <summary>
/// Bot type
/// </summary>
public enum BotType {
    /// <summary>
    /// A bot that randonly selects it's actions
    /// </summary>
    RANDOM,

    /// <summary>
    /// A bot that selects it's actions based on previous games
    /// </summary>
    SMART
}

/// <summary>
/// Bot configuration
/// </summary>
public class BotConfig {
    /// <summary>
    /// Text representation of the bot deck
    /// </summary>
    public required string StrDeck { get; set; }

    /// <summary>
    /// Bot name
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Bot type
    /// </summary>
    public required BotType BotType { get; set; }

    /// <summary>
    /// Action delay
    /// </summary>
    public int ActionDelay { get; set; } = 0;
}

/// <summary>
/// Player configuration
/// </summary>
public class PlayerConfig {
    /// <summary>
    /// Optional bot configuration
    /// </summary>
    public required BotConfig? BotConfig { get; set; }
}

/// <summary>
/// Match process configuration
/// </summary>
public class MatchProcessConfig {
    /// <summary>
    /// Match configuration ID
    /// </summary>
    [JsonPropertyName("mConfig")]
    public required string MatchConfigId { get; set; }

    /// <summary>
    /// Can other players watch flag
    /// </summary>
    public required bool CanWatch { get; set; }

    /// <summary>
    /// Match password
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    /// Player 1 configuration
    /// </summary>
    public required PlayerConfig P1Config { get; set; }

    /// <summary>
    /// Player 2 configuration
    /// </summary>
    public required PlayerConfig P2Config { get; set; }
}