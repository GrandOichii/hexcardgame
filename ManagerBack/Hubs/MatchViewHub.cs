using Microsoft.AspNetCore.SignalR;

namespace ManagerBack.Hubs;

public class MatchViewHub : Hub {
    private readonly IMatchService _matchService;

    public MatchViewHub(IMatchService matchService)
    {
        _matchService = matchService;
    }

    public static string ToGroupName(string matchId) => $"match-{matchId}";

    // TODO authorize
    // TODO prevent from invoking multiple times
    public async Task Connect(string matchId) {
        await Groups.AddToGroupAsync(Context.ConnectionId, ToGroupName(matchId));
    }

}