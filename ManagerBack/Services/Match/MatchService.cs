

using System.Net.WebSockets;
using System.Text;
using ManagerBack.Hubs;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;

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
    private readonly IHubContext<MatchLiveHub> _hubContext;
    public MatchService(ICardRepository cardRepo, IHubContext<MatchLiveHub> hubContext)
    {
        _cardMaster = new DBCardMaster(cardRepo);
        _hubContext = hubContext;
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
        var result = new MatchProcess(this, _cardMaster, config);
        await result.AddBots();
        _matches.Add(result.Id, result);
        await ServiceStatusUpdated(result);
        return result;
    }

    public Task<IEnumerable<MatchProcess>> All()
    {
        return Task.FromResult(_matches.Values.AsEnumerable());
    }

    public Task<MatchProcess> ById(string matchId)
    {
        var parsed = Guid.TryParse(matchId, out Guid guid);
        if (!parsed)
            throw new InvalidMatchIdException(matchId);
        // var guid = Guid.Parse(matchId);
        if (!_matches.ContainsKey(guid))
            throw new MatchNotFoundException(matchId);

        var match = _matches[guid];
        return Task.FromResult(match);
    }

    private readonly List<WatcherConnection> _watchers = new();

    public async Task ServiceStatusUpdated(MatchProcess match)
    {
        foreach (var watcher in _watchers) {
            // TODO check if disconnected
            await _hubContext.Clients.Client(watcher.ConnectionId).SendAsync("Update", match.ToJson());
        }
    }

    public async Task AddWatcher(string connectionId, string userId)
    {
        await _hubContext.Clients.Client(connectionId).SendAsync("Confirm");
        _watchers.Add(new(connectionId, userId));
    }

    public Task RemoveWatcher(string connectionId)
    {
        // TODO this seems unsafe
        var watcher = _watchers.FirstOrDefault(w => w.ConnectionId == connectionId);
        if (watcher is null)
            return Task.CompletedTask;
        _watchers.Remove(watcher);
        return Task.CompletedTask;
    }
}