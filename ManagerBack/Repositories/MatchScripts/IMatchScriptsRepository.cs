
namespace ManagerBack.Repositories;

/// <summary>
/// Repository of match scripts
/// </summary>
public interface IMatchScriptsRepository {
    /// <summary>
    /// Tries to fetch the core script
    /// </summary>
    /// <returns>Core script</returns>
    public Task<MatchScript?> GetCoreScript();
}