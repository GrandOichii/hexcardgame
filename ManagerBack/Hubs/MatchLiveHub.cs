using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using Utility;

namespace ManagerBack.Hubs;
public sealed class MatchLiveHub : Hub {
    private readonly IMatchService _matchServices;

    public MatchLiveHub(IMatchService matchServices, IHubContext<MatchLiveHub> hubContext)
    {
        _matchServices = matchServices;
    }

    public async Task Get() {
        var data = await _matchServices.All();
        foreach (var match in data) {
            await Clients.Client(Context.ConnectionId).SendAsync("Update", JsonSerializer.Serialize(match, Common.JSON_SERIALIZATION_OPTIONS));
        }
    }

}