namespace ManagerBack.Repositories;

public interface ICachedCardRepository {
    /// <summary>
    /// Adds the card to cache
    /// </summary>
    /// <param name="card">Card object</param>
    public Task Remember(CardModel card);

    /// <summary>
    /// Removes the card from cache
    /// </summary>
    /// <param name="cid">Card ID</param>
    public Task Forget(string cid);

    /// <summary>
    /// Tries to fetch the specified card from cache
    /// </summary>
    /// <param name="cid">Card ID</param>
    /// <returns>Card object if exists, else null</returns>
    public Task<CardModel?> Get(string cid);
}