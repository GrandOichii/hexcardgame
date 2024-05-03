using System.Linq.Expressions;
using ManagerBack.Controllers;
using MongoDB.Driver;

namespace ManagerBack.Repositories;

/// <summary>
/// Repository of cards
/// </summary>
public interface ICardRepository {
    /// <summary>
    /// Tries to fetch a card by it's CID
    /// </summary>
    /// <param name="cid">Card ID</param>
    /// <returns>Card object if exists, else null</returns>
    public Task<CardModel?> ByCID(string cid);

    /// <summary>
    /// Add a card object to the repository
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    public Task Add(CardModel card);

    /// <summary>
    /// Removes the card from the repository
    /// </summary>
    /// <param name="cid">Card ID</param>
    /// <returns>Amount of cards deleted</returns>
    public Task<long> Delete(string cid);

    /// <summary>
    /// Fetches all the cards, contained in the repository
    /// </summary>
    /// <returns>Enumerable of card objects</returns>
    public Task<IEnumerable<CardModel>> All();

    /// <summary>
    /// Fetches the cards, based of the specified filter
    /// </summary>
    /// <param name="filter">Filter function</param>
    /// <returns>Enumerable of filtered card objects</returns>
    public Task<IEnumerable<CardModel>> Filter(Expression<Func<CardModel, bool>> filter);

    /// <summary>
    /// Updates a card object
    /// </summary>
    /// <param name="card">New card data</param>
    /// <returns>Amount of cards updated</returns>
    public Task<long> Update(CardModel card);

    /// <summary>
    /// Queries the cards, using a card query object
    /// </summary>
    /// <param name="query">Card query object</param>
    /// <returns>Enumerable of queried card objects</returns>
    public Task<IEnumerable<CardModel>> Query(CardQuery query);
}