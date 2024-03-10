namespace HexClient;

using System;
using System.Collections.Generic;
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

public class PlayerRecord {
    public required string Name { get; set; }
    public List<string> Actions { get; set; } = new();
}

public class MatchRecord {
    public string ExceptionMessage { get; set; } = "";
    public string InnerExceptionMessage { get; set; } = "";

    #nullable enable
    public string? WinnerName { get; set; }
    
    #nullable disable
    public List<PlayerRecord> Players { get; set; } = new();
}

public class MatchProcess {
	public MatchStatus Status { get; set; }

    #nullable enable
	public MatchRecord? Record { get; set; }
    #nullable disable

	public string TcpAddress { get; set; }
	public Guid Id { get; set; }
	
	public DateTime? StartTime { get; set; }
	public DateTime? EndTime { get; set; }
}
