using System.Linq.Expressions;

namespace ManagerBack.Tests.Mocks;

public class FakeDeckRepository : IDeckRepository
{
    public List<DeckModel> Decks { get; set; } = new();
    public async Task Add(DeckModel deck)
    {
        deck.Id = Decks.Count.ToString();
        Decks.Add(deck);
    }

    public async Task<DeckModel?> ById(string id)
    {
        return Decks.FirstOrDefault(d => d.Id == id);
    }

    public async Task<long> Delete(string id)
    {
        var deck = Decks.FirstOrDefault(d => d.Id == id);
        if (deck is null)
            return 0;
        Decks.Remove(deck);
        return 1;
    }

    public async Task<IEnumerable<DeckModel>> Filter(Expression<Func<DeckModel, bool>> filter)
    {
        var f = filter.Compile();
        return Decks.FindAll(f.Invoke);
    }

    public async Task<long> Update(string deckId, DeckModel update)
    {
        var count = await Delete(deckId);
        if (count == 0)
            return 0;
        Decks.Add(update);
        return 1;
    }
}
