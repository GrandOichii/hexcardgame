namespace ManagerBack.Settings;

/// <summary>
/// Application database settings
/// </summary>
public class StoreDatabaseSettings {
    /// <summary>
    /// Database conection string
    /// </summary>
    public required string ConnectionString { get; set; }

    /// <summary>
    /// Name of the database
    /// </summary>
    public required string DatabaseName { get; set; }

    /// <summary>
    /// Card collection name
    /// </summary>
    public required string CardCollectionName { get; set; }

    /// <summary>
    /// User collection name
    /// </summary>
    public required string UserCollectionName { get; set; }

    /// <summary>
    /// Deck collection name
    /// </summary>
    public required string DeckCollectionName { get; set; }

    /// <summary>
    /// Match scripts collection name
    /// </summary>
    public required string MatchScriptsCollectionName { get; set; }

    /// <summary>
    /// Match configuration collection name
    /// </summary>
    public required string MatchConfigCollectionName { get; set; }
}