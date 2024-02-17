
namespace ManagerBack.Services;

public class DBCardMaster : ICardMaster
{
    private readonly ICardRepository _cardRepo;

    public DBCardMaster(ICardRepository cardRepo)
    {
        _cardRepo = cardRepo;
    }

    public ExpansionCard Get(string id)
    {
        return _cardRepo.ByCID(id).GetAwaiter().GetResult()!;
    }

    public IEnumerable<ExpansionCard> GetAll()
    {
        return _cardRepo.All().GetAwaiter().GetResult();
    }
}