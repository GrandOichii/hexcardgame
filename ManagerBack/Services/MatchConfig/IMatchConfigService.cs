namespace ManagerBack.Services;

/// <summary>
/// Match configuration service
/// </summary>
public interface IMatchConfigService {
    /// <summary>
    /// Fetches all match configurations
    /// </summary>
    /// <returns>Enumerable of all match configurations</returns>
    public Task<IEnumerable<MatchConfigModel>> All();

    /// <summary>
    /// Add a new match configuration
    /// </summary>
    /// <param name="config">New match configuration data</param>
    /// <returns>New match configuration</returns>
    public Task<MatchConfigModel> Add(PostMatchConfigDto config);

    /// <summary>
    /// Fetches a match configuration by it's ID
    /// </summary>
    /// <param name="id">Match configuration ID</param>
    /// <returns>Fetched match configuration</returns>
    public Task<MatchConfigModel> ById(string id);

    /// <summary>
    /// Fetches the basic match configuration
    /// </summary>
    /// <returns>Basic match configuration</returns>
    public Task<MatchConfigModel> Basic();

    /// <summary>
    /// Updates an existing match configuration by it's name
    /// </summary>
    /// <param name="newConfig"></param>
    /// <returns></returns>
    public Task<MatchConfigModel> Update(PostMatchConfigDto newConfig);
}