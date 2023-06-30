using System.Text.Json;
using System.Text.Json.Serialization;
namespace core.cards;


/// <summary>
/// Card object, for storage in database
/// </summary>
public class Card
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "<no-name>";
    [JsonPropertyName("cost")]
    public int Cost { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }="<no-type>";
    [JsonPropertyName("expansion")]
    public string Expansion { get; set; }="<no-expansion>";
    [JsonPropertyName("text")]
    public string Text { get; set; }="";
    [JsonPropertyName("power")]
    public int Power { get; set; } = -1;
    [JsonPropertyName("life")]
    public int Life { get; set; } = -1;
    [JsonPropertyName("deckUsable")]
    public bool DeckUsable { get; set; } = true;
    [JsonPropertyName("script")]
    public string Script { get; set; }="error(\"NO CARD SCRIPT SPECIFIED\")";

    /// <summary>
    /// Returns ID of the card in the format of [expansion]::[name].
    /// </summary>
    /// <returns>ID of the card</returns>
    public string CID() {
        return Expansion + "::" + Name;
    }
}


public abstract class CardMaster
{
    /// <summary>
    /// Fetches the card with the specified card ID.
    /// </summary>
    /// <param name="id">Card ID</param>
    /// <returns>Card with the specified ID</returns>
    abstract public Card Get(string id);
}


// TODO don't know if this is ok, for testing purposes
public class FileCardMaster : CardMaster
{
    private static string MANIFEST_FILE = "manifest.json";

    public List<Card> Cards { get; }

    public FileCardMaster() {
        Cards = new();
    }

    /// <summary>
    /// Loads the cards from the specified directory. Has to contain a manifest.json file.
    /// </summary>
    /// <param name="dir">Directory of the cards.</param>
    /// <returns>The amount of cards loaded.</returns>
    public int LoadCardsFrom(string dir) {
        // read the manifest file
        var manifestFile = Path.Join(dir, MANIFEST_FILE);
        var manifest = File.ReadAllText(manifestFile);
        var cardDirs = JsonSerializer.Deserialize<List<string>>(manifest);
        if (cardDirs is null) {
            throw new Exception("Failed to load manifest file in " + manifestFile);
        }

        int result = 0;
        foreach (var cardDir in cardDirs) {
            var cardPath = Path.Join(dir, cardDir);
            var text = File.ReadAllText(cardPath);
            var card = JsonSerializer.Deserialize<Card>(text);
            if (card is null) {
                throw new Exception("Failed to deserialize card from " + cardPath);
            }

            Cards.Add(card);
            ++result;
        }
        return result;
    }

    public override Card Get(string id)
    {
        foreach (var card in Cards)
            if (card.CID() == id)
                return card;

        throw new Exception("Can't load card with ID " + id);
    }
}