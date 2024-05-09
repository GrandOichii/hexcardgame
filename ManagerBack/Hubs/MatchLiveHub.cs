using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using Utility;

namespace ManagerBack.Hubs;

/// <summary>
/// SignalR hub for viewing live updates of the match table
/// </summary>
public sealed class MatchLiveHub : Hub {
    /// <summary>
    /// Match process service
    /// </summary>
    private readonly IMatchService _matchService;

    /// <summary>
    /// Logger
    /// </summary>
    private readonly ILogger<MatchLiveHub> _logger;

    public MatchLiveHub(IMatchService matchService, ILogger<MatchLiveHub> logger)
    {
        _matchService = matchService;
        _logger = logger;
    }

    /// <summary>
    /// SignalR method, used for fetching match table data
    /// </summary>
    /// <returns></returns>
    public async Task Get() {
        var data = await _matchService.All();
        foreach (var match in data) {
            await Clients.Client(Context.ConnectionId).SendAsync("Update", JsonSerializer.Serialize(match, Common.JSON_SERIALIZATION_OPTIONS));
        }
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

        _logger.LogInformation("Add new match table watcher");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);

        _logger.LogInformation("Table watcher disconnected");
    }

}