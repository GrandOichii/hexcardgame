using HexCore.GameMatch.States;
using HexCore.GameMatch.View;
using MongoDB.Bson;

namespace ManagerBack;

public struct BaseMatchState {
    public string CurPlayerID { get; set; }
    public List<PlayerState> Players { get; set; } = new();
    public MapState Map { get; set; }

    public BaseMatchState(Match match) {
        CurPlayerID = match.CurrentPlayer.ID;
        foreach (var p in match.Players)
            Players.Add(new PlayerState(p));
        
        Map = new MapState(match.Map);
    }
}

public class ConnectedMatchView : IMatchView
{
    private readonly IMatchService _matchService;
    private readonly Guid _matchId; 

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
        var state = new BaseMatchState(match);
        await _matchService.UpdateView(_matchId.ToString(), state);
    }
}
