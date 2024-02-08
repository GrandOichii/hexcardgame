
namespace ManagerBack.Services;

[System.Serializable]
public class ExpansionNotFoundException : System.Exception
{
    public ExpansionNotFoundException(string expansion) : base($"expansion {expansion} not found") { }
}

public class ExpansionService : IExpansionService
{
    private readonly ICardRepository _cardRepo;

    public ExpansionService(ICardRepository cardRepo)
    {
        _cardRepo = cardRepo;
    }

    private async Task<Dictionary<string, int>> GetCounts() {
        var cards = await _cardRepo.All();
        var result = new Dictionary<string, int>();
        foreach (var card in cards) {
            if (!result.ContainsKey(card.Expansion)) {
                result[card.Expansion] = 0;
            }
            ++result[card.Expansion];
        }
        return result;
    }

    public async Task<IEnumerable<Expansion>> All()
    {
        var mappings = await GetCounts();
        var result = new List<Expansion>();
        foreach (var pair in mappings) {
            result.Add(new Expansion {
                Name = pair.Key,
                CardCount = pair.Value
            });
        }
        return result;
    }

    public async Task<Expansion> ByName(string expansion)
    {
        var mappings = await GetCounts();
        if (!mappings.ContainsKey(expansion)) {
            throw new ExpansionNotFoundException(expansion);
        }
        var count = mappings[expansion];
        return new Expansion {
            Name = expansion,
            CardCount = count
        };
    }
}
