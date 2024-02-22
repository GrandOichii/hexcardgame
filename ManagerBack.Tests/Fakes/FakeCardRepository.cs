using System.Linq.Expressions;

namespace ManagerBack.Tests.Mocks;

public class FakeCardRepository : ICardRepository
{
    public List<CardModel> Cards { get; set; } = new();


    public Task Add(CardModel card)
    {
        Cards.Add(card);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<CardModel>> All()
    {
        return Task.FromResult(Cards.AsEnumerable());
    }

    public Task<CardModel?> ByCID(string cid)
    {
        return Task.FromResult(Cards.FirstOrDefault(c => c.GetCID() == cid));
    }

    public Task<long> Delete(string cid)
    {
        var found = Cards.FirstOrDefault(c => c.GetCID() == cid);
        if (found is null)
            return Task.FromResult((long)0);
        Cards.Remove(found);
        return Task.FromResult((long)1);
    }

    public Task<IEnumerable<CardModel>> Filter(Expression<Func<CardModel, bool>> filter)
    {
        var f = filter.Compile();
        return Task.FromResult(Cards.FindAll(f.Invoke).AsEnumerable());
    }

    public async Task<long> Update(CardModel card)
    {
        var deleted = await Delete(card.GetCID());
        if (deleted == 0) return 0;
        Cards.Add(card);
        return 1;
    }
}
