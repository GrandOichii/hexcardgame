using MongoDB.Driver;

namespace ManagerBack.Repositories;

public interface IMatchConfigRepository {
    public Task<IEnumerable<MatchConfigModel>> All();
    public Task Add(MatchConfigModel config);
    public Task<MatchConfigModel?> ById(string id);

}