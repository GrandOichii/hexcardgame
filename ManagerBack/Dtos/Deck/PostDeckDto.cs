namespace ManagerBack.Models;

/// <summary>
/// Dto object for posting deck information
/// </summary>
public class PostDeckDto {
    /// <summary>
    /// Deck name
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// Deck description
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Card name to amount mapping
    /// </summary>
    public Dictionary<string, int> Index { get; set; } = new();

    /// <summary>
    /// Creates a DeckTemplate representation of the deck
    /// </summary>
    /// <returns>Deck template object</returns>
    public DeckTemplate ToDeckTemplate() {
        return new DeckTemplate {
            Descriptors = new() {
                { "name", Name },
                { "description", Description },
            },
            Index = Index            
        };
    }
}