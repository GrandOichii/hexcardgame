using System.Linq.Expressions;

namespace ManagerBack.Repositories;

/// <summary>
/// Deck repository
/// </summary>
public interface IDeckRepository {
    /// <summary>
    /// Fetches the decks, based of the specified filter
    /// </summary>
    /// <param name="filter">Filter function</param>
    /// <returns>Enumerable of filtered deck objects</returns>
    public Task<IEnumerable<DeckModel>> Filter(Expression<Func<DeckModel, bool>> filter);

    /// <summary>
    /// Add the deck to the repository
    /// </summary>
    /// <param name="deck">Deck object</param>
    public Task Add(DeckModel deck);

    /// <summary>
    /// Fetches the deck object by it's ID
    /// </summary>
    /// <param name="id">Deck ID</param>
    /// <returns>Deck object if exists, else null</returns>
    public Task<DeckModel?> ById(string id);

    /// <summary>
    /// Removes the deck object from the repository 
    /// </summary>
    /// <param name="id">Deck ID</param>
    /// <returns>Amount of deleted decks</returns>
    public Task<long> Delete(string id);

    /// <summary>
    /// Updates the deck object
    /// </summary>
    /// <param name="deckId">Deck ID</param>
    /// <param name="update">New deck data</param>
    /// <returns>Amount of decks updated</returns>
    public Task<long> Update(string deckId, DeckModel update);
}