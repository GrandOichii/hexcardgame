

using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using HexCore.GameMatch.States;
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
    private readonly IHubContext<MatchLiveHub> _liveHubContext;
    private readonly IHubContext<MatchViewHub> _viewHubContext;
    public MatchService(ICardRepository cardRepo, IHubContext<MatchLiveHub> hubContext, IHubContext<MatchViewHub> viewHubContext)
    {
        _cardMaster = new DBCardMaster(cardRepo);
        _liveHubContext = hubContext;
        _viewHubContext = viewHubContext;
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

    public async Task ServiceStatusUpdated(MatchProcess match)
    {
        var data = JsonSerializer.Serialize(match);
        await _liveHubContext.Clients.All.SendAsync("Update", data);
    }

    public async Task UpdateView(string matchId, BaseMatchState state)
    {
        var data = state.ToJson();
        var group = MatchViewHub.ToGroupName(matchId);
        await _viewHubContext.Clients.Group(group).SendAsync("Update", data);
    }

    public async Task EndView(string matchId)
    {
        var group = MatchViewHub.ToGroupName(matchId);
        await _viewHubContext.Clients.Group(group).SendAsync("EndView");
    }
}