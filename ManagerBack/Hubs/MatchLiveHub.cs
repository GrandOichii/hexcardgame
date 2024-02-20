using System.Reflection;
using Microsoft.AspNetCore.SignalR;

namespace ManagerBack.Hubs;
public sealed class MatchLiveHub : Hub {
    private readonly IMatchService _matchServices;

    public MatchLiveHub(IMatchService matchServices, IHubContext<MatchLiveHub> hubContext)
    {
        _matchServices = matchServices;
    }

    public override async Task OnConnectedAsync() {
        System.Console.WriteLine("New connection established");
        await _matchServices.AddWatcher(Context.ConnectionId, Context.UserIdentifier!);
        // await Clients.All.SendAsync("ReceiveMessage", $"{Context.ConnectionId} has joined");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _matchServices.RemoveWatcher(Context.ConnectionId);

        await base.OnDisconnectedAsync(exception);
    }
}