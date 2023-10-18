using core.cards;
using core.decks;
using core.match;
using core.manager;
using System.Text.Json.Serialization;
using System.Net.Sockets;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;

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

    public DeckTemplate? this[string deckName]
    {
        get
        {
            foreach (var deck in Decks)
                if (deck.GetDescriptor("name") == deckName)
                    return deck;
            return null;
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
    //static public ManagerContext Ctx { get; set; }
    static public CardMaster CMaster { get; set; }
    static public DeckManager DManager { get; set; }
    static public ConfigsManager CManager { get; set; }
    static public List<MatchTrace> MatchTraces { get; set; } = new();

    static public ManagerContext Ctx
    {
        get => new ManagerContext();
    }
}

public class DBCardMaster : CardMaster
{
    public override ExpansionCard Get(string cid)
    {
        var split = cid.Split("::");
        var expansion = split[0];
        var name = split[1];

        var result = Global.Ctx.ExpansionCards
            .Where(c => c.Card.Name == name && c.ExpansionNameKey == expansion)
            .Include(c => c.Card)
            .ToList();
        return result[0].ToCard();
    }

    public override IEnumerable<ExpansionCard> GetAll() {
        var cards = Global.Ctx.ExpansionCards.ToList();
        var result = new List<ExpansionCard>();

        foreach (var card in cards)
            result.Add(card.ToCard());

        return result;
    }
}
