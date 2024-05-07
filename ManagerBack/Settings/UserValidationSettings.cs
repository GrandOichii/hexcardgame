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

/// <summary>
/// Application user validation settings
/// </summary>
public class UserValidationSettings {
    /// <summary>
    /// Username restriction
    /// </summary>
    public required StringRestriction Username { get; set; }

    /// <summary>
    /// User password restriction
    /// </summary>
    public required StringRestriction Password { get; set; }
}