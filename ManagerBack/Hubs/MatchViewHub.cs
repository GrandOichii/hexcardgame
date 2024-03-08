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
    // public async Task Connect(string matchId, string jwtToken) {
    public async Task Connect(string matchId) {
        try {
            var match = await _matchService.ById(matchId);
            if (!match.Config.CanWatch) {
                await Clients.Caller.SendAsync("Forbidden");
                return;
            }
        } catch (InvalidMatchIdException e) {
            await Clients.Caller.SendAsync("ConnectFail", e.Message);
        } catch (MatchNotFoundException e) {
            await Clients.Caller.SendAsync("ConnectFail", e.Message);
        }

        await RemoveFromAll(Context.ConnectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, ToGroupName(matchId));
        await _matchService.SendMatchInfo(matchId, Context.ConnectionId);
        await _matchService.SendMatchState(matchId, Context.ConnectionId);
    }

    // TODO is this method exposed
    private async Task RemoveFromAll(string connectionId) {
        foreach (var match in await _matchService.All()) {
            var group = ToGroupName(match.Id.ToString());
            await Groups.RemoveFromGroupAsync(connectionId, group);
        }
    }
}