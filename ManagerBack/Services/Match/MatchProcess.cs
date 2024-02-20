using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using HexCore.GameMatch.View;
using ManagerBack.Hubs;
using Microsoft.VisualBasic;

namespace ManagerBack.Services;

public enum MatchStatus {
    WAITING_FOR_PLAYERS,
    IN_PROGRESS,
    FINISHED,
    CRASHED
}

public class MatchProcess {
    public MatchStatus Status { get; private set; } = MatchStatus.WAITING_FOR_PLAYERS;
    public MatchRecord? Record { get; private set; } = null;
    public string TcpAddress { get; set; }
    public Guid Id { get; }
    [JsonIgnore]
    public TcpListener TcpListener { get; } 

    private readonly Match _match;
    private int _realPlayerCount;
    private static readonly Dictionary<BotType, string> BOT_TYPE_PATH_MAP = new() {
        {BotType.RANDOM, "../bots/random.lua"},
        {BotType.SMART, "../bots/basic.lua"},
    };
    private readonly MatchProcessConfig _config;
    private readonly IMatchService _matchService;
    public ConnectedMatchView View { get; }

    public MatchProcess(IMatchService matchService, ICardMaster cMaster, MatchProcessConfig config)
    {
        _matchService = matchService;
        _config = config;
        Id = Guid.NewGuid();

        TcpListener = new TcpListener(IPAddress.Loopback, 0);
        TcpListener.Start();
        TcpAddress = ((IPEndPoint)TcpListener.LocalEndpoint).ToString();

        _match = new Match(Id.ToString(), config.MatchConfig, cMaster);
        View = new ConnectedMatchView(Id, matchService);
        _match.View = View;
        
        _match.InitialSetup("../HexCore/core.lua");
        // TODO fix the order of the players

        _realPlayerCount = 0;
    }

    public async Task SetStatus(MatchStatus status) {
        Status = status;
        await _matchService.ServiceStatusUpdated(this);
    }

    public async Task ConnectTcpPlayers() {
        // TODO keeps listening even after the match starts
        while (CanAddConnection() && !Started()) {
            await AddTCPConnection();
        }
    }

    public async Task AddBots() {
        foreach (var p in new List<PlayerConfig> {_config.P1Config, _config.P2Config}) {
            if (p.BotConfig is null) {
                ++_realPlayerCount;
                continue;
            }

            var controller = new LuaPlayerController(BOT_TYPE_PATH_MAP[p.BotConfig.BotType]);
            var deck = DeckTemplate.FromText(p.BotConfig.StrDeck);
            // TODO validate deck
            await _match.AddPlayer(p.BotConfig.Name, deck, controller);
        }
        if (!CanAddConnection()) {
            Run();
            return;
        }
        Task.Run(ConnectTcpPlayers);
    }

    public bool CanAddConnection() {
        return Status == MatchStatus.WAITING_FOR_PLAYERS && _realPlayerCount > 0;
    }

    public async Task AddWebSocketConnection(WebSocket socket) {
        // TODO change to username extracted from jwt
        await socket.Write("name");
        string name = await socket.Read();

        // TODO this allows any user to submit any deck, change this later to deckId
        await socket.Write("deck");
        var resp = await socket.Read();
        var deck = DeckTemplate.FromText(resp);
        // TODO validate deck

        var controller = new WebSocketPlayerController(socket);
        await AddPlayer(name, deck, controller, true);
    }

    public async Task AddTCPConnection() {
        // var task = TcpListener.AcceptTcpClientAsync();
        // var success = task.Wait(100);
        // if (!success) {
        //     return;
        // }
        // var client = task.Result;
        var client = TcpListener.AcceptTcpClient();
        // if (!)
        var controller = new TCPPlayerController(client, _match);
        await controller.Write("name");
        var name = await controller.Read();

        await controller.Write("deck");
        var deckRaw = await controller.Read();
        var deck = DeckTemplate.FromText(deckRaw);
        await AddPlayer(name, deck, controller, true);
    }

    private async Task AddPlayer(string name, DeckTemplate deck, IPlayerController controller, bool isReal)  {

        await _match.AddPlayer(name, deck, controller);
        if (isReal)
            --_realPlayerCount;
        if (CanAddConnection()) return;

        Run();
    }

    public bool Started() {
        return Status != MatchStatus.WAITING_FOR_PLAYERS;
    }

    private async Task Run() {
        await SetStatus(MatchStatus.IN_PROGRESS);
        Record = new();

        try {
            await _match.Start();
            await SetStatus(MatchStatus.FINISHED);
            Record.WinnerName = _match.Winner!.Name;
        } catch (Exception e) {
            await SetStatus(MatchStatus.CRASHED);
            Record.ExceptionMessage = e.Message;
            if (e.InnerException is not null)
                Record.InnerExceptionMessage = e.InnerException.Message;            
        }
    }
    
    public async Task Finish() {
        // TODO this seems wrong
        while (Status == MatchStatus.IN_PROGRESS) {
            await Task.Delay(200);
        }
    }

}