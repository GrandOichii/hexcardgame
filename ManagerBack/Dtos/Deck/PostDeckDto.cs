namespace ManagerBack.Models;

public class PostDeckDto {
    public required string Name { get; set; }
    public required string Description { get; set; }

    public Dictionary<string, int> Index { get; set; } = new();

    public DeckTemplate ToDeckTemplate() {
        return new DeckTemplate {
            Descriptors = new() {
                { "name", Name },
                { "description", Description },
            },
            Index = Index            
        };
    }
}