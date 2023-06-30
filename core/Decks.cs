using core.cards;

namespace core.decks;

public class DeckTemplate {
    static private string LINE_SPLITTER = "|";
    static private string AMOUNT_SPLITTER = "#";

    public Dictionary<string, int> Index { get; }

    public DeckTemplate() {
        Index = new();
    }

    /// <summary>
    /// Creates a deck template from text.
    /// </summary>
    /// <param name="text">Text of the deck</param>
    /// <returns>The deck template</returns>
    static public DeckTemplate FromText(string text) {
        var result = new DeckTemplate();
        // dev::Test Card 1#5|dev::Test Card 2#2
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
    public Deck ToDeck(CardMaster cMaster) {
        // TODO
        return new Deck();
    }
}

/// <summary>
/// Deck object, is used in the match as the player's deck of cards
/// </summary>
public class Deck {
    // TODO
}