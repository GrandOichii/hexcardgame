namespace ManagerBack.Models;

public class PostDeckDto {
    public required string Name { get; set; }
    public required string Description { get; set; }

    public Dictionary<string, int> Index { get; set; } = new();
}