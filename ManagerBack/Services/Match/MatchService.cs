

namespace ManagerBack.Services;

public class MatchService : IMatchService
{
    private readonly CardMaster _cardMaster;
    private readonly Dictionary<Guid, MatchProcess> _matches = new();
    public MatchService(ICardRepository cardRepo)
    {
        _cardMaster = new DBCardMaster(cardRepo);
    }


    public Task Connect(string matchId)
    {
        throw new NotImplementedException();
    }

    public async Task<MatchProcess> Create(MatchProcessConfig config)
    {
        var result = new MatchProcess(_cardMaster, config);
        _matches.Add(result.Id, result);
        return result;
    }

    public async Task<IEnumerable<MatchProcess>> All()
    {
        return _matches.Values;
    }
}