namespace HexCore.Cards.Masters;

/// <summary>
/// Card master entity, is used for card fetching
/// </summary>
public interface ICardMaster
{
    /// <summary>
    /// Fetches the card with the specified card CID.
    /// </summary>
    /// <param name="cid">Card CID</param>
    /// <returns>Card with the specified CID</returns>
    /// 
    public Task<ExpansionCard> Get(string cid);
    
    /// <summary>
    /// Fetches all cards
    /// </summary>
    /// <returns><Container of all cards/returns>
    public Task<IEnumerable<ExpansionCard>> GetAll();
}