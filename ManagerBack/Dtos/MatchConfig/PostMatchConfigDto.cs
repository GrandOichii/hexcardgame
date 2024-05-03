namespace ManagerBack.Dtos;

/// <summary>
/// Dto for posting new match configurations
/// </summary>
public class PostMatchConfigDto : MatchConfig {
    /// <summary>
    /// Match configuration name
    /// </summary>
    public required string Name { get; set; }
}