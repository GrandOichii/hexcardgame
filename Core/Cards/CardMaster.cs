using System.Text.Json;

namespace Core.Cards;


/// <summary>
/// Card master entity, is used for card fetching
/// </summary>
public interface ICardMaster
{
    /// <summary>
    /// Fetches the card with the specified card ID.
    /// </summary>
    /// <param name="id">Card ID</param>
    /// <returns>Card with the specified ID</returns>
    /// 
    public ExpansionCard Get(string id);
    
    /// <summary>
    /// Fetches all cards
    /// </summary>
    /// <returns><Container of all cards/returns>
    public IEnumerable<ExpansionCard> GetAll();
}


/// <summary>
/// File card master, loads cards using a manifest file
/// </summary>
public class FileCardMaster : ICardMaster
{
    private static string MANIFEST_FILE = "manifest.json";

    public List<ExpansionCard> Cards { get; }

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
            if (cardDir[0] == '!') continue;
            
            var cardPath = Path.Join(dir, cardDir);
            var text = File.ReadAllText(cardPath);
            var card = JsonSerializer.Deserialize<ExpansionCard>(text);
            if (card is null) {
                throw new Exception("Failed to deserialize card from " + cardPath);
            }

            Cards.Add(card);
            ++result;
        }
        return result;
    }

    public ExpansionCard Get(string id)
    {
        foreach (var card in Cards)
            if (card.CID == id)
                return card;

        throw new Exception("Can't load card with ID " + id);
    }

    public IEnumerable<ExpansionCard> GetAll() => Cards;
}

