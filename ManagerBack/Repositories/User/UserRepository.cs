
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ManagerBack.Repositories;

/// <summary>
/// Implementation of the IUserRepository, uses a MongoDB collection as the data source
/// </summary>
public class UserRepository : IUserRepository
{
    /// <summary>
    /// MongoDB user collection
    /// </summary>
    private readonly IMongoCollection<User> _collection;

    public UserRepository(IOptions<StoreDatabaseSettings> pollStoreDatabaseSettings) {
        _collection = new MongoClient(
            pollStoreDatabaseSettings.Value.ConnectionString
        ).GetDatabase(
            pollStoreDatabaseSettings.Value.DatabaseName
        ).GetCollection<User>(
            pollStoreDatabaseSettings.Value.UserCollectionName
        );
    }

    public async Task Add(User user)
    {
        await _collection.InsertOneAsync(user);
    }

    public async Task<User?> ByUsername(string username)
    {
        var result = await _collection.FindAsync(u => u.Username == username);
        return result.FirstOrDefault();
    }

    public async Task<bool> CheckId(string id) {
        var result = await _collection.CountDocumentsAsync(u => u.Id == id);
        return result == 1;
    }
}