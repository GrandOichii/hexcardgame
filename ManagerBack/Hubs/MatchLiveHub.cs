using System.Reflection;
using Microsoft.AspNetCore.SignalR;

namespace ManagerBack.Hubs;
public sealed class MatchLiveHub : Hub {
    private readonly IMatchService _matchServices;

    public MatchLiveHub(IMatchService matchServices, IHubContext<MatchLiveHub> hubContext)
    {
        _matchServices = matchServices;
    }

    // TODO add some message handlers

}