namespace ManagerBack.Repositories;

public interface ICachedMatchConfigRepository {
    public Task Remember(MatchConfigModel config);
    public Task Forget(string id);
    public Task<MatchConfigModel?> Get(string id);
}