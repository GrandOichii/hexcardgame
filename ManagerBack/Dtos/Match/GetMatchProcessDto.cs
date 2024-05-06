namespace ManagerBack.Dtos;

/// <summary>
/// Dto for fetching matches
/// </summary>
public class GetMatchProcessDto {
    /// <summary>
    /// Match status
    /// </summary>
    public MatchStatus Status { get; set; }
    
    /// <summary>
    /// User ID of the match creator
    /// </summary>
    public required string CreatorId { get; set; }

    /// <summary>
    /// Match ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Start time of the match
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// End time of the match
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Match configuration information
    /// </summary>
    public required MatchProcessConfig Config { get; set; }

    /// <summary>
    /// Array of potential match participants
    /// </summary>
    public required QueuedPlayer[] QueuedPlayers { get; set; }

    /// <summary>
    /// The recording of the match
    /// </summary>
    public required MatchRecord Record { get; set; }

    /// <summary>
    /// Match TCP port for connecting
    /// </summary>
    public required int TcpPort { get; set; }

    /// <summary>
    /// Password requirement flag
    /// </summary>
    public required bool PasswordRequired { get; set; }
}

