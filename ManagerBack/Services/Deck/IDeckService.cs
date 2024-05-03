using ManagerBack.Dtos;

namespace ManagerBack.Services;

/// <summary>
/// Deck interface
/// </summary>
public interface IDeckService {
    /// <summary>
    /// Fetches all of the specified user's decks
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Enumerable of the user's decks</returns>
    public Task<IEnumerable<DeckModel>> All(string userId);

    /// <summary>
    /// Adds a new user deck
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="deck">Deck data</param>
    /// <returns>New deck information</returns>
    public Task<DeckModel> Add(string userId, PostDeckDto deck);

    /// <summary>
    /// Removes the user deck
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="deckId">Deck ID</param>
    public Task Delete(string userId, string deckId);

    /// <summary>
    /// Updates an existing user deck
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="deckId">Deck ID</param>
    /// <param name="deck">New deck data</param>
    /// <returns>New deck information</returns>
    public Task<DeckModel> Update(string userId, string deckId, PostDeckDto deck);
}