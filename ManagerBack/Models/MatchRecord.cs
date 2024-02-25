namespace ManagerBack.Models;

public class PlayerRecord {
    public required string Name { get; set; }
    public List<string> Actions { get; set; } = new();
}

public class MatchRecord {
    public required MatchProcessConfig Config { get; set; }
    public string ExceptionMessage { get; set; } = "";
    public string InnerExceptionMessage { get; set; } = "";
    public string? WinnerName { get; set; }
    public List<PlayerRecord> Players { get; set; } = new();
}