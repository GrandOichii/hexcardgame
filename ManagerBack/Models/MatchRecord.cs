namespace ManagerBack.Models;

public class MatchRecord {
    public string ExceptionMessage { get; set; } = "";
    public string InnerExceptionMessage { get; set; } = "";
    public string? WinnerName { get; set; }
    
}