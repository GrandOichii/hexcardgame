


using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ManagerBack.Repositories;

public class MatchScriptsRepository : IMatchScriptsRepository
{
    private readonly IMongoCollection<MatchScript> _collection;
    private readonly ICachedCardRepository _cachedCards;

    public MatchScriptsRepository(IOptions<StoreDatabaseSettings> pollStoreDatabaseSettings, ICachedCardRepository cachedCards)
    {
        _collection = new MongoClient(
            pollStoreDatabaseSettings.Value.ConnectionString
        ).GetDatabase(
            pollStoreDatabaseSettings.Value.DatabaseName
        ).GetCollection<MatchScript>(
            pollStoreDatabaseSettings.Value.MatchScriptsCollectionName
        );

        _cachedCards = cachedCards;
    }

    public async Task<MatchScript?> GetCoreScript()
    {
        var script = await _collection.FindAsync(s => s.Name == "core");
        return script.FirstOrDefault();
    }
}