using System.Text.Json;
using System.Text.Json.Serialization;

namespace HexCore.Decks;

[Serializable]
public class DeckParseException : Exception
{
    public DeckParseException() { }
    public DeckParseException(string message) : base(message) { }
}

public class DeckTemplate {
    private static readonly string LINE_SPLITTER = "|";
    private static readonly string AMOUNT_SPLITTER = "#";
    private static readonly string DESCRIPTORS_SPLITTER = ";";
    private static readonly string DESCRIPTOR_PARTS_SPLITTER = "|";

    public Dictionary<string, int> Index { get; set; } = new();
    public Dictionary<string, string> Descriptors { get; set; } = new();

    public DeckTemplate() {
    }

    /// <summary>
    /// Creates a deck template from text.
    /// </summary>
    /// <param name="text">Text of the deck</param>
    /// <returns>The deck template</returns>
    static public DeckTemplate FromText(string text) {
        // dev::Test Card 1#5|dev::Test Card 2#2;name=deck1,description=this is the deck's description
        
        var result = new DeckTemplate();
        var gLines = text.Split(DESCRIPTORS_SPLITTER);

        if (gLines.Length == 2) {
            // add descriptions
            var descriptors = gLines[1].Split(DESCRIPTOR_PARTS_SPLITTER);
            foreach (var dLine in descriptors) {
                var s = dLine.Split("=");
                result.Descriptors.Add(s[0], s[1]);
                // dev::Dub#3|dev::Urakshi Raider#3|dev::Elven Outcast#3;name=deck1,description=This is a simple deck, used for testing.
            }
        }

        text = gLines[0];
        var lines = text.Split(LINE_SPLITTER);
        foreach (var line in lines) {
            var split = line.Split(AMOUNT_SPLITTER);
            if (split.Length != 2) {
                throw new DeckParseException("Failed to parse card line " + line);
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
        var result = string.Join(LINE_SPLITTER, lines);
        if (Descriptors.Count > 0) {
            result += DESCRIPTORS_SPLITTER;
            lines.Clear();
            foreach (var pair in Descriptors) {
                lines.Add($"{pair.Key}={pair.Value}");
            }
            result += string.Join(DESCRIPTOR_PARTS_SPLITTER, lines);
        }
        return result;
    }

    /// <summary>
    /// Creates a deck object from the template
    /// </summary>
    /// <param name="cMaster">Card master object, is used for fetching cards</param>
    /// <returns>Deck, created from template</returns>
    public async Task<Zone<MatchCard>> ToDeck(Match match, Player owner) {
        var list = new List<MatchCard>();

        var cm = match.CardMaster;
        foreach (var pair in Index) {
            var cardID = pair.Key;
            var card = await cm.Get(cardID);
            var amount = pair.Value;
            for (int i = 0; i < amount; i++) {
                var mCard = new MatchCard(match, card, owner);
                list.Add(mCard);
            }
        }

        var result = new Zone<MatchCard>(list);
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

    /// <summary>
    /// Sets the descriptor of the deck
    /// </summary>
    /// <param name="name">Name of the descriptor</param>
    /// <param name="value">New descriptor value</param>
    public void SetDescriptor(string name, string value) {
        if (!Descriptors.ContainsKey(name)) {
            Descriptors.Add(name, value);
            return;
        }
        Descriptors[name] = value;
    }

    /// <summary>
    /// Returns the JSON format of the deck
    /// </summary>
    /// <returns>JSON format of the deck</returns>
    public string ToJson() {
        return JsonSerializer.Serialize(this, Common.JSON_SERIALIZATION_OPTIONS);
    }
}