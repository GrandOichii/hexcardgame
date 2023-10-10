using core.cards;
using core.decks;

public class DeckManager {
    public List<DeckTemplate> Decks { get; set; } = new();
    public DeckManager(string loadPath) {
        var files = Directory.GetFiles(loadPath);
        int nameI = 1;
        foreach (var file in files) {
            var deckText = File.ReadAllText(file);
            var deckTemplate = DeckTemplate.FromText(deckText);
            if (deckTemplate.GetDescriptor("name").Length == 0) {
                deckTemplate.Descriptors["name"] = "deck" + nameI;
                ++nameI;
            }
            Decks.Add(deckTemplate);
        }
    }
}

public class Global {
    static public CardMaster CMaster { get; set; }
    static public DeckManager DManager { get; set; }
}