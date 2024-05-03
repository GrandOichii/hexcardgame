using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ManagerBack.Repositories;

/// <summary>
/// Implementation of the IMatchConfigRepository, uses a MongoDB collection as the data source
/// </summary>
public class MatchConfigRepository : IMatchConfigRepository {
    /// <summary>
    /// MongoDB match configuration collection
    /// </summary>
    private readonly IMongoCollection<MatchConfigModel> _collection;

    /// <summary>
    /// Cached match configurations repository
    /// </summary>
    private readonly ICachedMatchConfigRepository _cachedConfigs;

    public MatchConfigRepository(IOptions<StoreDatabaseSettings> pollStoreDatabaseSettings, ICachedMatchConfigRepository cachedConfigs)
    {
        _collection = new MongoClient(
            pollStoreDatabaseSettings.Value.ConnectionString
        ).GetDatabase(
            pollStoreDatabaseSettings.Value.DatabaseName
        ).GetCollection<MatchConfigModel>(
            pollStoreDatabaseSettings.Value.MatchConfigCollectionName
        );
        _cachedConfigs = cachedConfigs;
    }

    public async Task Add(MatchConfigModel config)
    {
        await _collection.InsertOneAsync(config);
        await _cachedConfigs.Remember(config);
    }

    public async Task<IEnumerable<MatchConfigModel>> All()
    {
        var result = await _collection.FindAsync(_ => true);
        return await result.ToListAsync();
    }

    public async Task<MatchConfigModel?> ById(string id)
    {
        var cached = await _cachedConfigs.Get(id);
        if (cached is not null) {
            return cached;
        }

        try {
            var found = await _collection.FindAsync(c => c.Id == id);
            var result = await found.FirstOrDefaultAsync();
            return result;
        } catch (FormatException) {
            return null;
        }
    }

    public async Task<IEnumerable<MatchConfigModel>> Filter(Expression<Func<MatchConfigModel, bool>> filter)
    {
        return (await _collection.FindAsync(filter)).ToList();
    }
}