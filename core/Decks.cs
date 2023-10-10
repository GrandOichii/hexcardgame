using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using core.cards;
using core.match;
using core.players;

namespace core.decks;

public class DeckTemplate {
    static private string LINE_SPLITTER = "|";
    static private string AMOUNT_SPLITTER = "#";
    static private string DESCRIPTORS_SPLITTER = ";";
    static private string DESCRIPTOR_PARTS_SPLITTER = ",";

    [JsonPropertyName("index")]
    public Dictionary<string, int> Index { get; set; }
    [JsonPropertyName("descriptors")]
    public Dictionary<string, string> Descriptors { get; set; }=new();

    public DeckTemplate() {
        Index = new();
    }

    /// <summary>
    /// Creates a deck template from text.
    /// </summary>
    /// <param name="text">Text of the deck</param>
    /// <returns>The deck template</returns>
    static public DeckTemplate FromText(string text) {
        // dev::Test Card 1#5|dev::Test Card 2#2;name=deck1,description=Amogus Amogus Amogus
        
        var result = new DeckTemplate();
        var gLines = text.Split(DESCRIPTORS_SPLITTER);

        if (gLines.Length == 2) {
            // add descriptions
            var descriptors = gLines[1].Split(DESCRIPTOR_PARTS_SPLITTER);
            foreach (var dLine in descriptors) {
                var s = dLine.Split("=");
                result.Descriptors.Add(s[0], s[1]);
            }
        }

        text = gLines[0];
        var lines = text.Split(LINE_SPLITTER);
        foreach (var line in lines) {
            var split = line.Split(AMOUNT_SPLITTER);
            if (split.Length != 2) {
                throw new Exception("Failed to parse card line " + line);
            }
            var cid = split[0];
            var amount = int.Parse(split[1]);
            result.Index.Add(cid, amount);
        }
        return result;
    }

    /// <summary>
    /// Returns a short text with deck contents
    /// </summary>
    /// <returns>Text with deck contents</returns>
    public string ToText() {
        var lines = new List<string>();
        foreach (var pair in Index) {
            var line = pair.Key + AMOUNT_SPLITTER + pair.Value;
            lines.Add(line);
        }
        return String.Join(LINE_SPLITTER, lines);
    }

    /// <summary>
    /// Creates a deck object from the template
    /// </summary>
    /// <param name="cMaster">Card master object, is used for fetching cards</param>
    /// <returns>Deck, created from template</returns>
    public Zone<MCard> ToDeck(core.match.Match match, Player owner) {
        var list = new List<MCard>();

        var cm = match.CardMaster;
        foreach (var pair in Index) {
            var cardID = pair.Key;
            var card = cm.Get(cardID);
            var amount = pair.Value;
            for (int i = 0; i < amount; i++) {
                var mCard = new MCard(match, card, owner);
                list.Add(mCard);
            }
        }

        var result = new Zone<MCard>(list);
        return result;
    }

    /// <summary>
    /// Gets the descriptor of the deck
    /// </summary>
    /// <param name="name">Name of the descriptor</param>
    /// <returns>If contains descriptor, returns it's value, else returns empty string</returns>
    public string GetDescriptor(string name) {
        if (Descriptors.ContainsKey(name)) return Descriptors[name];
        return "";
    }
}