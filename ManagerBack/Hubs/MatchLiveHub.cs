using System.Reflection;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;

namespace ManagerBack.Hubs;
public sealed class MatchLiveHub : Hub {
    private readonly IMatchService _matchServices;

    public MatchLiveHub(IMatchService matchServices, IHubContext<MatchLiveHub> hubContext)
    {
        _matchServices = matchServices;
    }

    public async Task Get() {
        var data = await _matchServices.All();
        await Clients.Client(Context.ConnectionId).SendAsync(data.ToJson());
    }

}