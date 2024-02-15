
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
        // TODO test
        return _cardRepo.ByCID(id).GetAwaiter().GetResult()!;
    }

    public override IEnumerable<ExpansionCard> GetAll()
    {
        // TODO test
        return _cardRepo.All().GetAwaiter().GetResult();
    }
}