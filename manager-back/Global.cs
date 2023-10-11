using core.cards;
using core.decks;
using core.match;
using core.manager;

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



public class ConfigsManager
{
    public List<ManagerMatchConfig> Configs { get; set; } = new();
    public ConfigsManager(string loadPath)
    {
        var files = Directory.GetFiles(loadPath);
        foreach (var file in files)
        {
            if (Path.GetExtension(file) != ".json") continue;
            var name = Path.GetFileNameWithoutExtension(file);

            var c = new ManagerMatchConfig();
            c.Config = MatchConfig.FromJson(File.ReadAllText(file));
            c.Name = name;

            Configs.Add(c);
        }
    }
}

public class Global {
    static public CardMaster CMaster { get; set; }
    static public DeckManager DManager { get; set; }
    static public ConfigsManager CManager { get; set; }
}