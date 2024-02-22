using System.Linq.Expressions;

namespace ManagerBack.Tests.Mocks;

public class FakeDeckRepository : IDeckRepository
{
    public List<DeckModel> Decks { get; set; } = new();
    public Task Add(DeckModel deck)
    {
        deck.Id = Decks.Count.ToString();
        Decks.Add(deck);
        return Task.CompletedTask;
    }

    public Task<DeckModel?> ById(string id)
    {
        return Task.FromResult(Decks.FirstOrDefault(d => d.Id == id));
    }

    public Task<long> Delete(string id)
    {
        var deck = Decks.FirstOrDefault(d => d.Id == id);
        if (deck is null)
            return Task.FromResult((long)0);
        Decks.Remove(deck);
        return Task.FromResult((long)1);
    }

    public Task<IEnumerable<DeckModel>> Filter(Expression<Func<DeckModel, bool>> filter)
    {
        var f = filter.Compile();
        return Task.FromResult(Decks.FindAll(f.Invoke).AsEnumerable());
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
