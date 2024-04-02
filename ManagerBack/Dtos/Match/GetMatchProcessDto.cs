namespace ManagerBack.Dtos;

public class GetMatchProcessDto {
    public MatchStatus Status { get; set; }
    public required string CreatorId { get; set; }
    public Guid Id { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public required MatchProcessConfig Config { get; set; }
    public required QueuedPlayer[] QueuedPlayers { get; set; }
    // !FIXME hide seed while match is in progress
    public required MatchRecord Record { get; set; }
    public required int TcpPort { get; set; }
}