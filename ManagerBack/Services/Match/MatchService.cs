
using System.Text.Json;
using AutoMapper;
using HexCore.GameMatch.States;
using Microsoft.AspNetCore.SignalR;
using Utility;

namespace ManagerBack.Services;

/// <summary>
/// Exception that indicates that the match refused to connect a player
/// </summary>
[Serializable]
public class MatchRefusedConnectionException : Exception
{
    public MatchRefusedConnectionException() { }
    public MatchRefusedConnectionException(string matchId) : base($"match with id {matchId} refused connection") { }
}

/// <summary>
/// Exception that indicates that that specified match ID is invalid
/// </summary>
[Serializable]
public class InvalidMatchIdException : Exception
{
    public InvalidMatchIdException() { }
    public InvalidMatchIdException(string matchId) : base($"invalid match id: {matchId}") { }
}

/// <summary>
/// Exception that indicates that a match with the specified ID doesn't exist
/// </summary>
[Serializable]
public class MatchNotFoundException : Exception
{
    public MatchNotFoundException() { }
    public MatchNotFoundException(string matchId) : base($"no match with id {matchId}") { }
}

/// <summary>
/// Implementation of the IMatchService interface
/// </summary>
public class MatchService : IMatchService
{
    /// <summary>
    /// Match ID to match process mapping
    /// </summary>
    public Dictionary<Guid, MatchProcess> _matches = new();

    /// <summary>
    /// User repository
    /// </summary>
    private readonly IUserRepository _userRepo;

    /// <summary>
    /// Mapper object
    /// </summary>
    private readonly IMapper _mapper;

    /// <summary>
    /// SignalR match table view hub context
    /// </summary>
    private readonly IHubContext<MatchLiveHub> _liveHubContext;

    /// <summary>
    /// SignalR match view hub context
    /// </summary>
    private readonly IHubContext<MatchViewHub> _viewHubContext;

    /// <summary>
    /// SignalR match process view hub context
    /// </summary>
    private readonly IHubContext<MatchProcessHub> _matchProcessHub;

    /// <summary>
    /// Match configuration repository
    /// </summary>
    private readonly IMatchConfigRepository _configRepo;

    /// <summary>
    /// Card master
    /// </summary>
    private readonly ICardMaster _cardMaster;

    /// <summary>
    /// Match scripts repository
    /// </summary>
    private readonly IMatchScriptsRepository _scriptsRepo;

    public MatchService(IMapper mapper, IHubContext<MatchLiveHub> hubContext, IHubContext<MatchViewHub> viewHubContext, IMatchConfigRepository configRepo, ICardRepository cardRepo, IHubContext<MatchProcessHub> matchProcessHub, IUserRepository userRepo, IMatchScriptsRepository scriptsRepo)
    {
        _mapper = mapper;
        _liveHubContext = hubContext;
        _viewHubContext = viewHubContext;
        _configRepo = configRepo;
        _cardMaster = new DBCardMaster(cardRepo);
        _matchProcessHub = matchProcessHub;
        _userRepo = userRepo;
        _scriptsRepo = scriptsRepo;
    }

    /// <summary>
    /// Fetches a match using an ID
    /// </summary>
    /// <param name="guid">Match ID</param>
    /// <returns>The match process</returns>
    /// <exception cref="MatchNotFoundException"></exception>
    private Task<MatchProcess> GetMatch(Guid guid) {
        if (!_matches.ContainsKey(guid))
            throw new MatchNotFoundException(guid.ToString());

        var match = _matches[guid];
        return Task.FromResult(
            match
        );
    }

    /// <summary>
    /// Fetches a match using an ID
    /// </summary>
    /// <param name="id">Match ID</param>
    /// <returns>The match process</returns>
    /// <exception cref="MatchNotFoundException"></exception>
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

    private readonly object _matchCreateLock = new();
    public async Task<GetMatchProcessDto> Create(string userId, MatchProcessConfig config)
    {
        var mConfig = await _configRepo.ById(config.MatchConfigId)
            ?? throw new MatchConfigNotFoundException($"no match config with id {config.MatchConfigId}")
        ;
        
        var userExists = await _userRepo.CheckId(userId);
        if (!userExists)
            throw new UserNotFoundException($"no user with id {userId}");

        var match = new MatchProcess(userId, config, mConfig, _cardMaster, this, _scriptsRepo);
        match.Changed += OnMatchProcessChanged;

        lock (_matchCreateLock) {
            _matches.Add(match.Id, match);
        }

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
        lock (_matchCreateLock) {
            var newMatches = new Dictionary<Guid, MatchProcess>();
            foreach (var pair in _matches) {
                var m = pair.Value;
                if (filter.Invoke(m)) continue;

                newMatches.Add(pair.Key, m);
            }
            _matches = newMatches;
        }

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
        
        var state = match.View.LastState;
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

    /// <summary>
    /// Converts the match ID to a SignalR hub group name
    /// </summary>
    /// <param name="matchId">Match ID</param>
    /// <returns>Group name</returns>
    private static string ToGroupName(string matchId) => $"watchers-{matchId}";

    public async Task RegisterWatcher(string matchId, string connectionId)
    {
        try {
            var _ = await GetMatch(matchId);
        } catch (Exception e) when (e is InvalidMatchIdException || e is MatchNotFoundException) {
            await _matchProcessHub.Clients.Client(connectionId).SendAsync("Refused");
            return;
        }

        await _matchProcessHub.Groups.AddToGroupAsync(connectionId, ToGroupName(matchId));

        await UpdateWatchers(matchId);
    }

    /// <summary>
    /// Updates all match watcher
    /// </summary>
    /// <param name="matchId">Match ID</param>
    private async Task UpdateWatchers(string matchId) {
        var match = await GetMatch(matchId);

        var matchDto = _mapper.Map<GetMatchProcessDto>(match);
        await _matchProcessHub.Clients.Group(ToGroupName(matchId)).SendAsync("Update", JsonSerializer.Serialize(matchDto, Common.JSON_SERIALIZATION_OPTIONS));
    }

    /// <summary>
    /// Event handler for match process changing
    /// </summary>
    /// <param name="matchId">Match ID</param>
    private async Task OnMatchProcessChanged(string matchId) {
        await UpdateWatchers(matchId);
    }
}