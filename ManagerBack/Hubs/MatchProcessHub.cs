using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using Utility;

namespace ManagerBack.Hubs;

/// <summary>
/// SignalR hub for viewing the status of a single match process
/// </summary>
public sealed class MatchProcessHub : Hub {
    /// <summary>
    /// Match process service
    /// </summary>
    private readonly IMatchService _matchService;

    public MatchProcessHub(IMatchService matchService)
    {
        _matchService = matchService;
    }

    /// <summary>
    /// SignalR method, used for connecting to the specified match process
    /// </summary>
    /// <param name="matchId">Match process ID</param>
    public async Task Connect(string matchId) {
        await _matchService.RegisterWatcher(matchId, Context.ConnectionId);
    }
}