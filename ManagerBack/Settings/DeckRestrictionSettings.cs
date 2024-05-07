namespace ManagerBack.Settings;

/// <summary>
/// Application deck restriction settings
/// </summary>
public class DeckRestrictionSettings {
    /// <summary>
    /// Maximum amount of decks a player can have
    /// </summary>
    public required int MaxDeckCount { get; set; }

    /// <summary>
    /// Deck name restrictions
    /// </summary>
    public required StringRestriction Name { get; set; }
}