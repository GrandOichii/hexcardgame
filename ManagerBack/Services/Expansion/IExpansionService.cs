namespace ManagerBack.Services;

/// <summary>
/// Expansion service
/// </summary>
public interface IExpansionService {
    /// <summary>
    /// Fetches all expansions
    /// </summary>
    /// <returns>Enumerable of all expansions</returns>
    public Task<IEnumerable<Expansion>> All();

    /// <summary>
    /// Fetches a card expansion by name
    /// </summary>
    /// <param name="expansion">Expansion name</param>
    /// <returns></returns>
    public Task<Expansion> ByName(string expansion);
}