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

    /// <summary>
    /// Logger
    /// </summary>
    private readonly ILogger<MatchProcessHub> _logger;

    public MatchProcessHub(IMatchService matchService, ILogger<MatchProcessHub> logger)
    {
        _matchService = matchService;
        _logger = logger;
    }

    /// <summary>
    /// SignalR method, used for connecting to the specified match process
    /// </summary>
    /// <param name="matchId">Match process ID</param>
    public async Task Connect(string matchId) {
        _logger.LogInformation("Connection {@connection} requested to view match process {@matchId}", Context.ConnectionId, matchId);
        await _matchService.RegisterWatcher(matchId, Context.ConnectionId);
    }
}