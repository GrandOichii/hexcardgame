namespace ManagerBack.Models;

// ? turn this into a DB model or move to a different directory

/// <summary>
/// A recording of the player information and their actions
/// </summary>
public class PlayerRecord {
    /// <summary>
    /// Player namme
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Text representation of the player's deck
    /// </summary>
    public required string Deck { get; set; }

    /// <summary>
    /// List of player actions
    /// </summary>
    public List<string> Actions { get; set; } = new();
}


/// <summary>
/// The record of the match
/// </summary>
public class MatchRecord {
    /// <summary>
    /// Match process configuration
    /// </summary>
    public required MatchProcessConfig Config { get; set; }

    /// <summary>
    /// Error message
    /// </summary>
    public string ExceptionMessage { get; set; } = "";

    /// <summary>
    /// Inner error message
    /// </summary>
    public string InnerExceptionMessage { get; set; } = "";

    /// <summary>
    /// Name of the winner player
    /// </summary>
    public string? WinnerName { get; set; }

    /// <summary>
    /// List of recorded players
    /// </summary>
    public List<PlayerRecord> Players { get; set; } = new();

    /// <summary>
    /// Match seed
    /// </summary>
    public int Seed { get; set; }

    // ? redundant
    /// <summary>
    /// ID of the match configuration
    /// </summary>
    public required string ConfigId { get; set; }
}