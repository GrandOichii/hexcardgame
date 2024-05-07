namespace ManagerBack.Settings;

/// <summary>
/// Restriction object for strings
/// </summary>
public class StringRestriction {
    /// <summary>
    /// String minimum length
    /// </summary>
    public required int MinLength { get; set; }

    /// <summary>
    /// String maximum length
    /// </summary>
    public required int MaxLength { get; set; }
}