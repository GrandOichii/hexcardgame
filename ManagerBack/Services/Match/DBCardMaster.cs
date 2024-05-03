
namespace ManagerBack.Services;

/// <summary>
/// Implementation of the ICardMaster interface, uses the ICardRepository injected object
/// </summary>
public class DBCardMaster : ICardMaster
{
    /// <summary>
    /// Card repository
    /// </summary>
    private readonly ICardRepository _cardRepo;

    public DBCardMaster(ICardRepository cardRepo)
    {
        _cardRepo = cardRepo;
    }

    public async Task<ExpansionCard> Get(string cid)
    {
        var result = await _cardRepo.ByCID(cid);
        return result!;
    }

    public async Task<IEnumerable<ExpansionCard>> GetAll()
    {
        return await _cardRepo.All();
    }
}