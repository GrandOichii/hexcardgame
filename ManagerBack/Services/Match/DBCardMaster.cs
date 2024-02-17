
namespace ManagerBack.Services;

public class DBCardMaster : CardMaster
{
    private readonly ICardRepository _cardRepo;

    public DBCardMaster(ICardRepository cardRepo)
    {
        _cardRepo = cardRepo;
    }

    public override ExpansionCard Get(string id)
    {
        return _cardRepo.ByCID(id).GetAwaiter().GetResult()!;
    }

    public override IEnumerable<ExpansionCard> GetAll()
    {
        return _cardRepo.All().GetAwaiter().GetResult();
    }
}