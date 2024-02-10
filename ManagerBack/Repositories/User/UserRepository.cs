
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ManagerBack.Repositories;

public class UserRepository : IUserRepository
{
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
}