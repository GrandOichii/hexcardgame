
namespace ManagerBack.Repositories;

public interface IMatchScriptsRepository {
    public Task<MatchScript?> GetCoreScript();
}