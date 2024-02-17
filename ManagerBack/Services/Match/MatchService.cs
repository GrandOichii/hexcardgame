

using System.Net.WebSockets;
using System.Text;

namespace ManagerBack.Services;

[Serializable]
public class InvalidMatchIdException : Exception
{
    public InvalidMatchIdException() { }
    public InvalidMatchIdException(string matchId) : base($"invalid match id: {matchId}") { }
}

[Serializable]
public class MatchNotFoundException : Exception
{
    public MatchNotFoundException() { }
    public MatchNotFoundException(string matchId) : base($"no match with id {matchId}") { }
}

[Serializable]
public class MatchRefusedConnectionException : Exception
{
    public MatchRefusedConnectionException() { }
    public MatchRefusedConnectionException(string matchId) : base($"match with id {matchId} refused connection") { }
}

public class MatchService : IMatchService
{
    private readonly ICardMaster _cardMaster;
    private readonly Dictionary<Guid, MatchProcess> _matches = new();
    public MatchService(ICardRepository cardRepo)
    {
        _cardMaster = new DBCardMaster(cardRepo);
    }

    private async Task<MatchProcess> GetMatch(string matchId) {
        var match = await ById(matchId);

        if (!match.CanAddConnection())
            throw new MatchRefusedConnectionException(matchId);
        return match;
    }

    public async Task TCPConnect(string userId, string matchId) {
        var match = await GetMatch(matchId);
        await match.AddTCPConnection();
    }

    public async Task WSConnect(WebSocketManager manager, string userId, string matchId)
    {
        var match = await GetMatch(matchId);
        
        var socket = await manager.AcceptWebSocketAsync();
        string resp;

        await match.AddWebSocketConnection(socket);

        while (!match.Started()) {
            await socket.Write("playerwaiting");
            resp = await socket.Read(); 
            // TODO check response
        }
        
        await socket.Write("matchstart");
        resp = await socket.Read();
        // TODO check response

        await match.Finish();
    }

    public async Task<MatchProcess> Create(string userId, MatchProcessConfig config)
    {
        var result = new MatchProcess(_cardMaster, config);
        _matches.Add(result.Id, result);
        return result;
    }

    public async Task<IEnumerable<MatchProcess>> All()
    {
        return _matches.Values;
    }

    public async Task<MatchProcess> ById(string matchId)
    {
        var parsed = Guid.TryParse(matchId, out Guid guid);
        if (!parsed)
            throw new InvalidMatchIdException(matchId);
        // var guid = Guid.Parse(matchId);
        if (!_matches.ContainsKey(guid))
            throw new MatchNotFoundException(matchId);

        var match = _matches[guid];
        return match;
    }
}