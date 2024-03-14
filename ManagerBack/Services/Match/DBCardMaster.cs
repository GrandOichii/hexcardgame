
namespace ManagerBack.Services;

public class DBCardMaster : ICardMaster
{
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