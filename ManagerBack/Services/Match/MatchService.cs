
using System.Text.Json;
using AutoMapper;
using HexCore.GameMatch.States;
using Microsoft.AspNetCore.SignalR;
using Utility;

namespace ManagerBack.Services;

[Serializable]
public class MatchRefusedConnectionException : Exception
{
    public MatchRefusedConnectionException() { }
    public MatchRefusedConnectionException(string matchId) : base($"match with id {matchId} refused connection") { }
}

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

public class MatchService : IMatchService
{
    public Dictionary<Guid, MatchProcess> _matches = new();

    private readonly IMapper _mapper;
    private readonly IHubContext<MatchLiveHub> _liveHubContext;
    private readonly IHubContext<MatchViewHub> _viewHubContext;
    private readonly IMatchConfigRepository _configRepo;
    private readonly ICardMaster _cardMaster;

    public MatchService(IMapper mapper, IHubContext<MatchLiveHub> hubContext, IHubContext<MatchViewHub> viewHubContext, IMatchConfigRepository configRepo, ICardRepository cardRepo)
    {
        _mapper = mapper;
        _liveHubContext = hubContext;
        _viewHubContext = viewHubContext;
        _configRepo = configRepo;
        _cardMaster = new DBCardMaster(cardRepo);
    }

    private Task<MatchProcess> GetMatch(Guid guid) {
        if (!_matches.ContainsKey(guid))
            throw new MatchNotFoundException(guid.ToString());

        var match = _matches[guid];
        return Task.FromResult(
            match
        );
    }

    private async Task<MatchProcess> GetMatch(string id) {
        var parsed = Guid.TryParse(id, out Guid guid);
        if (!parsed)
            throw new InvalidMatchIdException(id);
        return await GetMatch(guid);
    }

    public Task<IEnumerable<GetMatchProcessDto>> All()
    {
        return Task.FromResult(
            _matches.Values.Select(_mapper.Map<GetMatchProcessDto>)
        );
    }

    public async Task<GetMatchProcessDto> ById(string matchId)
    {
        var result = await GetMatch(matchId);
        return 
            _mapper.Map<GetMatchProcessDto>(result)
        ;
    }

    public async Task<GetMatchProcessDto> Create(string userId, MatchProcessConfig config)
    {
        var mConfig = await _configRepo.ById(config.MatchConfigId)
            ?? throw new MatchConfigNotFoundException($"no match config with id {config.MatchConfigId}")
        ;
        
        // TODO check user id
        var match = new MatchProcess(userId, config, mConfig, _cardMaster, this);
        _matches.Add(match.Id, match);

        _ = match.InitialSetup();
        await ServiceStatusUpdated(match);

        return _mapper.Map<GetMatchProcessDto>(
            match
        );
    }

    public async Task EndView(string matchId)
    {
        var match = await GetMatch(matchId);
        var group = MatchViewHub.ToGroupName(matchId);

        await _viewHubContext.Clients.Group(group).SendAsync("EndView", match.Status, match.Match!.Winner);

    }

    public async Task Remove(Func<MatchProcess, bool> filter)
    {
        var newMatches = new Dictionary<Guid, MatchProcess>();
        foreach (var pair in _matches) {
            var m = pair.Value;
            if (filter.Invoke(m)) continue;

            newMatches.Add(pair.Key, m);
        }
        _matches = newMatches;

        var data = JsonSerializer.Serialize(_matches.Values.Select(_mapper.Map<GetMatchProcessDto>), Common.JSON_SERIALIZATION_OPTIONS);
        await _liveHubContext.Clients.All.SendAsync("UpdateAll", data); 
    }

    public async Task SendMatchInfo(string matchId, string connectionId)
    {
        var match = await GetMatch(matchId);
        var info = new MatchInfoState(match.Match!);
        var data = JsonSerializer.Serialize(info, Common.JSON_SERIALIZATION_OPTIONS);
        await _viewHubContext.Clients.Client(connectionId).SendAsync("Config", data);
    }

    public async Task SendMatchState(string matchId, string connectionId)
    {
        var match = await GetMatch(matchId);
        var state = new BaseMatchState(match.Match!);
        var data = JsonSerializer.Serialize(state, Common.JSON_SERIALIZATION_OPTIONS);
        await _viewHubContext.Clients.Client(connectionId).SendAsync("Update", data);
    }

    public async Task ServiceStatusUpdated(MatchProcess match)
    {
        var matchDto = _mapper.Map<GetMatchProcessDto>(match);
        var data = JsonSerializer.Serialize(matchDto, Common.JSON_SERIALIZATION_OPTIONS);
        await _liveHubContext.Clients.All.SendAsync("Update", data);
    }

    public async Task UpdateView(string matchId, BaseMatchState state)
    {
        var data = JsonSerializer.Serialize(state, Common.JSON_SERIALIZATION_OPTIONS);
        var group = MatchViewHub.ToGroupName(matchId);
        await _viewHubContext.Clients.Group(group).SendAsync("Update", data);
    }

    public async Task WSConnect(WebSocketManager manager, string userId, string matchId)
    {
        var match = await GetMatch(matchId);

        if (!match.CanConnect())
            throw new MatchRefusedConnectionException(matchId);

        var socket = await manager.AcceptWebSocketAsync();
        await match.AddWebSocketConnection(socket);
        await match.Finish(socket);
    }
}