
using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ManagerBack.Repositories;

/// <summary>
/// Implementation of the IDeckRepository, uses a MongoDB collection as the data source
/// </summary>
public class DeckRepository : IDeckRepository
{
    /// <summary>
    /// MongoDB deck collection
    /// </summary>
    private readonly IMongoCollection<DeckModel> _collection;

    public DeckRepository(IOptions<StoreDatabaseSettings> pollStoreDatabaseSettings) {
        _collection = new MongoClient(
            pollStoreDatabaseSettings.Value.ConnectionString
        ).GetDatabase(
            pollStoreDatabaseSettings.Value.DatabaseName
        ).GetCollection<DeckModel>(
            pollStoreDatabaseSettings.Value.DeckCollectionName
        );
    }

    public async Task Add(DeckModel deck)
    {
        await _collection.InsertOneAsync(deck);
    }

    public async Task<DeckModel?> ById(string id)
    {
        return (await _collection.FindAsync(d => d.Id == id)).FirstOrDefault();
    }

    public async Task<long> Delete(string id)
    {
        var result = await _collection.DeleteOneAsync(d => d.Id == id);
        return result.DeletedCount;
    }

    public async Task<IEnumerable<DeckModel>> Filter(Expression<Func<DeckModel, bool>> filter)
    {
        return (await _collection.FindAsync(filter)).ToList();
    }

    public async Task<long> Update(string deckId, DeckModel update)
    {
        var result = await _collection.ReplaceOneAsync(d => d.Id == deckId, update);
        return result.MatchedCount;
    }
}