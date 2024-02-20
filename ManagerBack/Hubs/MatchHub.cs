using System.Reflection;
using Microsoft.AspNetCore.SignalR;

namespace ManagerBack.Hubs;
public sealed class MatchHub : Hub {
    private readonly IMatchService _matchServices;
    private readonly IHubContext<MatchHub> _hubContext;

    public MatchHub(IMatchService matchServices, IHubContext<MatchHub> hubContext)
    {
        _matchServices = matchServices;
        _hubContext = hubContext;
    }

    
}