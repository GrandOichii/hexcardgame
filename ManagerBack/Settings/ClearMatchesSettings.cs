namespace ManagerBack.Settings;

/// <summary>
/// Settings for ClearMatchesTask background task
/// </summary>
public class ClearMatchesSettings {
    /// <summary>
    /// Flag for enabling match clearing
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Period between match clearing, in ms
    /// </summary>
    public required int Timeout { get; set; }

    /// <summary>
    /// Flag for clearing crashed matches
    /// </summary>
    public bool ClearCrashed { get; set; } = true;

    /// <summary>
    /// Flag for clearing finished matches
    /// </summary>
    public bool ClearFinished { get; set; } = true;
}