using System.Text.Json;

namespace HexCore.Cards.Masters;

[Serializable]
class UnknownCardException : Exception
{
    public UnknownCardException() { }
    public UnknownCardException(string message) : base(message) { }
}

/// <summary>
/// File card master, loads cards using a manifest file
/// </summary>
public class FileCardMaster : ICardMaster
{
    private static readonly string MANIFEST_FILE = "manifest.json";

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
        var cardDirs = JsonSerializer.Deserialize<List<string>>(manifest) ?? throw new Exception("Failed to load manifest file in " + manifestFile);
        int result = 0;
        foreach (var cardDir in cardDirs) {
            if (cardDir[0] == '!') continue;
            
            var cardPath = Path.Join(dir, cardDir);
            var text = File.ReadAllText(cardPath);
            var card = JsonSerializer.Deserialize<ExpansionCard>(text) ?? throw new Exception("Failed to deserialize card from " + cardPath);
            Cards.Add(card);
            ++result;
        }
        return result;
    }

    public Task<ExpansionCard> Get(string cid)
    {
        foreach (var card in Cards)
            if (card.GetCID() == cid)
                return Task.FromResult(card);
        
        throw new UnknownCardException("Can't load card with ID " + cid);
    }

    public Task<IEnumerable<ExpansionCard>> GetAll() => Task.FromResult(Cards.AsEnumerable());
}

