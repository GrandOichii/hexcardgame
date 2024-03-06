namespace HexClient;

using System;
using System.ComponentModel;

public enum MatchStatus {
	WAITING_FOR_PLAYERS,
	IN_PROGRESS,
	FINISHED,
	CRASHED
}

public static class MatchStatusExtensions {
	public static string ToFriendlyString(this MatchStatus status) {
        return status switch
        {
            MatchStatus.WAITING_FOR_PLAYERS => "Waiting for players",
            MatchStatus.IN_PROGRESS => "In progress",
            MatchStatus.FINISHED => "Finished",
            MatchStatus.CRASHED => "Crashed",
            _ => "unknown status",
        };
    }
}

public class MatchProcess {
	public MatchStatus Status { get; set; }
	// public MatchRecord? Record { get; private set; } = null;
	public string TcpAddress { get; set; }
	public Guid Id { get; set; }
	
	public DateTime? StartTime { get; set; }
	public DateTime? EndTime { get; set; }
}
