using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ManagerBack.Repositories;

public class CardRepository : ICardRepository {
    private readonly IMongoCollection<CardModel> _collection;

    public CardRepository(IOptions<StoreDatabaseSettings> pollStoreDatabaseSettings) {
        _collection = new MongoClient(
            pollStoreDatabaseSettings.Value.ConnectionString
        ).GetDatabase(
            pollStoreDatabaseSettings.Value.DatabaseName
        ).GetCollection<CardModel>(
            pollStoreDatabaseSettings.Value.CardCollectionName
        );
    }

    public async Task Add(CardModel card)
    {
        await _collection.InsertOneAsync(card);
    }


    public async Task<IEnumerable<CardModel>> Filter(System.Linq.Expressions.Expression<Func<CardModel, bool>> filter) {
        return (await _collection.FindAsync(filter)).ToList();
    }

    public async Task<IEnumerable<CardModel>> All()
    {
        return (await _collection.FindAsync(_ => true)).ToList();
    }

    public async Task<CardModel?> ByCID(string cid) {
        var found = await _collection.FindAsync(c => c.Expansion + "::" + c.Name == cid);
        var result = await found.FirstOrDefaultAsync();
        return result;
    }

    public async Task<long> Delete(string cid)
    {
        var deleted = await _collection.DeleteOneAsync(c => c.Expansion + "::" + c.Name == cid);
        return deleted.DeletedCount;
    }

    public async Task<long> Update(CardModel card) {
        var result = await _collection.ReplaceOneAsync(c => c.Expansion == card.Expansion && c.Name == card.Name, card);
        return result.MatchedCount;
    }
}