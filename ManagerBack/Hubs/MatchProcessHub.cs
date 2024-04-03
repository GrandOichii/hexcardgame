using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using Utility;

namespace ManagerBack.Hubs;
public sealed class MatchProcessHub : Hub {
    private readonly IMatchService _matchService;

    public MatchProcessHub(IMatchService matchService)
    {
        _matchService = matchService;
    }

    public async Task Connect(string matchId) {
        await _matchService.RegisterWatcher(matchId, Context.ConnectionId);
    }
}