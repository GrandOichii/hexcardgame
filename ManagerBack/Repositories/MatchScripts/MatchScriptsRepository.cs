


using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ManagerBack.Repositories;

/// <summary>
/// Implementation of the IMatchScriptsRepository, uses a MongoDB collection as the data source
/// </summary>
public class MatchScriptsRepository : IMatchScriptsRepository
{
    /// <summary>
    /// MongoDB match scripts collection
    /// </summary>
    private readonly IMongoCollection<MatchScript> _collection;

    public MatchScriptsRepository(IOptions<StoreDatabaseSettings> pollStoreDatabaseSettings)
    {
        _collection = new MongoClient(
            pollStoreDatabaseSettings.Value.ConnectionString
        ).GetDatabase(
            pollStoreDatabaseSettings.Value.DatabaseName
        ).GetCollection<MatchScript>(
            pollStoreDatabaseSettings.Value.MatchScriptsCollectionName
        );
    }

    public async Task<MatchScript?> GetCoreScript()
    {
        var script = await _collection.FindAsync(s => s.Name == "core");
        return script.FirstOrDefault();
    }
}