namespace ManagerBack.Settings;

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