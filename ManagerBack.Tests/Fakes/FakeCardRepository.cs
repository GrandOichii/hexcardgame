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

    public Task<IEnumerable<CardModel>> Query(CardQuery query)
    {
        var result = new List<CardModel>();
        foreach (var card in Cards ) 
            if (
                card.Name.ToLower().Contains(query.Name.ToLower()) &&
                card.Type.ToLower().Contains(query.Type.ToLower()) &&
                card.Text.ToLower().Contains(query.Text.ToLower()) &&
                card.Text.ToLower() == query.Text.ToLower()
            ) result.Add(card);
        

        return Task.FromResult(result.AsEnumerable());
    
        // var filter = 
        //     builder.Regex(c => c.Name, new Regex(query.Name.ToLower(), RegexOptions.IgnoreCase)) &
        //     builder.Regex(c => c.Type, new Regex(query.Type.ToLower(), RegexOptions.IgnoreCase)) &
        //     builder.Regex(c => c.Text, new Regex(query.Text.ToLower(), RegexOptions.IgnoreCase)) &
        //     builder.Regex(c => c.Expansion, new Regex(query.Expansion.ToLower(), RegexOptions.IgnoreCase))
        // ;
        // return await _collection.Find(filter).ToListAsync();
    }

    public async Task<long> Update(CardModel card)
    {
        var deleted = await Delete(card.GetCID());
        if (deleted == 0) return 0;
        Cards.Add(card);
        return 1;
    }
}
