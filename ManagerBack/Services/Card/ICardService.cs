using ManagerBack.Controllers;

namespace ManagerBack.Services;

/// <summary>
/// Card service
/// </summary>
public interface ICardService {
    /// <summary>
    /// Gets all unique card IDs
    /// </summary>
    /// <returns>Enumerable of card IDs</returns>
    public Task<IEnumerable<string>> AllCIDs();    

    /// <summary>
    /// Fetches the card by CID
    /// </summary>
    /// <param name="cid">Card ID</param>
    /// <returns>Card information</returns>
    public Task<ExpansionCard> ByCID(string cid);

    /// <summary>
    /// Adds a new card
    /// </summary>
    /// <param name="card">Card object</param>
    /// <returns>Added card information</returns>
    public Task<ExpansionCard> Add(ExpansionCard card);

    /// <summary>
    /// Updates a card
    /// </summary>
    /// <param name="card">New card data</param>
    public Task Update(ExpansionCard card);

    /// <summary>
    /// Deletes the card using it's ID
    /// </summary>
    /// <param name="cid">Card ID</param>
    public Task Delete(string cid);

    /// <summary>
    /// Fetches all the cards
    /// </summary>
    /// <returns>Enumerable of all the cards</returns>
    public Task<IEnumerable<ExpansionCard>> All();

    /// <summary>
    /// Queries the cards using the query object
    /// </summary>
    /// <param name="query">Query object</param>
    /// <returns>Enumerable of the queried cards</returns>
    public Task<IEnumerable<ExpansionCard>> Query(CardQuery query);
}