using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ManagerBack.Hubs;

/// <summary>
/// SignalR hub for viewing a specified match in action
/// </summary>
public class MatchViewHub : Hub {
    /// <summary>
    /// Match service
    /// </summary>
    private readonly IMatchService _matchService;

    private readonly ILogger<MatchViewHub> _logger;

    public MatchViewHub(IMatchService matchService, ILogger<MatchViewHub> logger)
    {
        _matchService = matchService;
        _logger = logger;
    }

    /// <summary>
    /// Converts match ID to a group name
    /// </summary>
    /// <param name="matchId">Match ID</param>
    /// <returns>Group name</returns>
    public static string ToGroupName(string matchId) => $"match-{matchId}";

    /// <summary>
    /// SignalR method for registering the caller to watch a match
    /// </summary>
    /// <param name="matchId">Match ID</param>
    [Authorize]
    public async Task Connect(string matchId) {
        GetMatchProcessDto match;
        try {
            match = await _matchService.ById(matchId);
            if (!match.Config.CanWatch) {
                await Clients.Caller.SendAsync("Forbidden");
                return;
            }
        } catch (InvalidMatchIdException e) {
            await Clients.Caller.SendAsync("ConnectFail", e.Message);
            return;
        } catch (MatchNotFoundException e) {
            await Clients.Caller.SendAsync("ConnectFail", e.Message);
            return;
        }

        await RemoveFromAll(Context.ConnectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, ToGroupName(matchId));

        _logger.LogInformation("Add {@connection} to watcher list of match {@matchId}", Context.ConnectionId, matchId);

        if (match.Status != MatchStatus.IN_PROGRESS) {
            return;
        }
        await _matchService.SendMatchInfo(matchId, Context.ConnectionId);
        await _matchService.SendMatchState(matchId, Context.ConnectionId);
    }

    /// <summary>
    /// Removes the specified connection from all match viewing groups
    /// </summary>
    /// <param name="connectionId">Connection ID</param>
    private async Task RemoveFromAll(string connectionId) {
        foreach (var match in await _matchService.All()) {
            var group = ToGroupName(match.Id.ToString());
            await Groups.RemoveFromGroupAsync(connectionId, group);
        }
    }
}