using HexCore.GameMatch.States;
using HexCore.GameMatch.View;
using MongoDB.Bson;

namespace ManagerBack;

/// <summary>
/// Base match state
/// </summary>
public struct BaseMatchState {
    /// <summary>
    /// ID of current player
    /// </summary>
    public string CurPlayerID { get; set; }

    /// <summary>
    /// List of player states
    /// </summary>
    public List<PlayerState> Players { get; set; } = new();

    /// <summary>
    /// Match map
    /// </summary>
    public MapState Map { get; set; }

    public BaseMatchState(Match match) {
        CurPlayerID = match.CurrentPlayer.ID;
        foreach (var p in match.Players)
            Players.Add(new PlayerState(p));
        
        Map = new MapState(match.Map);
    }
}

/// <summary>
/// Implementation of IMatchView interface, uses a IMatchService to send match info to users
/// </summary>
public class ConnectedMatchView : IMatchView
{
    /// <summary>
    /// Match service
    /// </summary>
    private readonly IMatchService _matchService;

    /// <summary>
    /// Observed match ID
    /// </summary>
    private readonly Guid _matchId;

    /// <summary>
    /// Last remembered state of the match
    /// </summary>
    public BaseMatchState LastState { get; private set; }

    public ConnectedMatchView(Guid matchId, IMatchService matchService)
    {
        _matchService = matchService;
        _matchId = matchId;
    }

    public async Task End()
    {
        await _matchService.EndView(_matchId.ToString());
    }

    public Task Start()
    {
        return Task.CompletedTask;
    }

    public async Task Update(Match match)
    {
        LastState = new BaseMatchState(match);
        await _matchService.UpdateView(_matchId.ToString(), LastState);
    }
}
