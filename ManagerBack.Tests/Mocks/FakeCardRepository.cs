using System.Linq.Expressions;

namespace ManagerBack.Tests.Mocks;

public class FakeCardRepository : ICardRepository
{
    public List<CardModel> Cards { get; set; } = new();


    public async Task Add(CardModel card)
    {
        Cards.Add(card);
    }

    public async Task<IEnumerable<CardModel>> All()
    {
        return Cards;
    }

    public async Task<CardModel?> ByCID(string cid)
    {
        return Cards.FirstOrDefault(c => c.GetCID() == cid);
    }

    public async Task<long> Delete(string cid)
    {
        var found = Cards.FirstOrDefault(c => c.GetCID() == cid);
        if (found is null)
            return 0;
        Cards.Remove(found);
        return 1;
    }

    public async Task<IEnumerable<CardModel>> Filter(Expression<Func<CardModel, bool>> filter)
    {
        var f = filter.Compile();
        return Cards.FindAll(f.Invoke);
    }

    public async Task<long> Update(CardModel card)
    {
        var deleted = await Delete(card.GetCID());
        if (deleted == 0) return 0;
        Cards.Add(card);
        return 1;
    }
}
