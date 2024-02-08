
namespace ManagerBack.Services;

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
            result.Add(new Expansion(){
                Name = pair.Key,
                CardCount = pair.Value
            });
        }
        return result;
    }

    public Task<Expansion> ByName()
    {
        throw new NotImplementedException();
    }
}
