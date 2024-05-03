namespace ManagerBack.Models;

// ? move this to a different directory

/// <summary>
/// Expansion object
/// </summary>
public class Expansion {
    /// <summary>
    /// Expansion name
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Card count
    /// </summary>
    public required int CardCount { get; set; }
}