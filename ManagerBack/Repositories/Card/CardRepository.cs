using System.Linq.Expressions;
using System.Text.RegularExpressions;
using ManagerBack.Controllers;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ManagerBack.Repositories;

/// <summary>
/// Implementation of the ICardRepository, uses a MongoDB collection as the data source
/// </summary>
public class CardRepository : ICardRepository {
    /// <summary>
    /// MongoDB card collection
    /// </summary>
    private readonly IMongoCollection<CardModel> _collection;

    /// <summary>
    /// Cached card repository
    /// </summary>
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
        string cid = card.GetCID();
        var found = await _collection.FindAsync(c => c.Expansion + "::" + c.Name == cid);
        var existing = await found.FirstOrDefaultAsync();
        if (existing is null)
            return 0;
        card.Id = existing.Id;
        var result = await _collection.ReplaceOneAsync(c => c.Expansion == card.Expansion && c.Name == card.Name, card);
        await _cachedCards.Remember(card);
        return result.MatchedCount;
    }

    public async Task<IEnumerable<CardModel>> Query(CardQuery query)
    {
        var builder = Builders<CardModel>.Filter;
        
        var filter = 
            builder.Regex(c => c.Name, new Regex(query.Name.ToLower(), RegexOptions.IgnoreCase)) &
            builder.Regex(c => c.Type, new Regex(query.Type.ToLower(), RegexOptions.IgnoreCase)) &
            builder.Regex(c => c.Text, new Regex(query.Text.ToLower(), RegexOptions.IgnoreCase)) &
            builder.Regex(c => c.Expansion, new Regex(query.Expansion.ToLower(), RegexOptions.IgnoreCase))
        ;
        return await _collection.Find(filter).ToListAsync();
    }
}