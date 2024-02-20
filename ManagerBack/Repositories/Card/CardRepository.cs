using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ManagerBack.Repositories;

public class CardRepository : ICardRepository {
    private readonly IMongoCollection<CardModel> _collection;
    private readonly ICachedCardRepository _cachedCards;

    public CardRepository(IOptions<StoreDatabaseSettings> pollStoreDatabaseSettings, ICachedCardRepository cachedCards)
    {
        _collection = new MongoClient(
            pollStoreDatabaseSettings.Value.ConnectionString
        ).GetDatabase(
            pollStoreDatabaseSettings.Value.DatabaseName
        ).GetCollection<CardModel>(
            pollStoreDatabaseSettings.Value.CardCollectionName
        );

        _cachedCards = cachedCards;
    }

    public async Task Add(CardModel card)
    {
        await _collection.InsertOneAsync(card);
        await _cachedCards.Remember(card);
    }


    public async Task<IEnumerable<CardModel>> Filter(Expression<Func<CardModel, bool>> filter) {
        return (await _collection.FindAsync(filter)).ToList();
    }

    public async Task<IEnumerable<CardModel>> All()
    {
        return (await _collection.FindAsync(_ => true)).ToList();
    }

    public async Task<CardModel?> ByCID(string cid) {
        var cached = await _cachedCards.Get(cid);
        if (cached is not null) {
            return cached;
        }
        var found = await _collection.FindAsync(c => c.Expansion + "::" + c.Name == cid);
        var result = await found.FirstOrDefaultAsync();
        if (result is not null)
            await _cachedCards.Remember(result);
        return result;
    }

    public async Task<long> Delete(string cid)
    {
        await _cachedCards.Forget(cid);
        var deleted = await _collection.DeleteOneAsync(c => c.Expansion + "::" + c.Name == cid);
        return deleted.DeletedCount;
    }

    public async Task<long> Update(CardModel card) {
        var result = await _collection.ReplaceOneAsync(c => c.Expansion == card.Expansion && c.Name == card.Name, card);
        await _cachedCards.Remember(card);
        return result.MatchedCount;
    }
}