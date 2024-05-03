namespace ManagerBack.Repositories;

/// <summary>
/// Match configuration repository, used for caching
/// </summary>
public interface ICachedMatchConfigRepository {
    /// <summary>
    /// Adds the match configuration to cache
    /// </summary>
    /// <param name="config">Match configuration object</param>
    public Task Remember(MatchConfigModel config);

    /// <summary>
    /// Removes the match configuration from cache
    /// </summary>
    /// <param name="id">Match configuration ID</param>
    public Task Forget(string id);

    /// <summary>
    /// Tries to fetch the match configuration from cache
    /// </summary>
    /// <param name="id">Match configuration ID</param>
    /// <returns>Match configueration if exists, else null</returns>
    public Task<MatchConfigModel?> Get(string id);
}