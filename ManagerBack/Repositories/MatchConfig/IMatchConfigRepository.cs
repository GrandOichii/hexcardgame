using System.Linq.Expressions;
using MongoDB.Driver;

namespace ManagerBack.Repositories;

/// <summary>
/// Match configuration repository
/// </summary>
public interface IMatchConfigRepository {
    /// <summary>
    /// Fetches all match configurations
    /// </summary>
    /// <returns></returns>
    public Task<IEnumerable<MatchConfigModel>> All();

    /// <summary>
    /// Adds a new match configuration to the repository
    /// </summary>
    /// <param name="config">Match configuration</param>
    public Task Add(MatchConfigModel config);

    /// <summary>
    /// Tries to fetch the match configuration by its ID
    /// </summary>
    /// <param name="id">Match configuration ID</param>
    /// <returns>Match configuration if exists, else null</returns>
    public Task<MatchConfigModel?> ById(string id);

    /// <summary>
    /// Fetches the match configurations, based of the specified filter
    /// </summary>
    /// <param name="filter">Filter function</param>
    /// <returns>Enumerable of filtered match configurations</returns>
   public Task<IEnumerable<MatchConfigModel>> Filter(Expression<Func<MatchConfigModel, bool>> filter);

}