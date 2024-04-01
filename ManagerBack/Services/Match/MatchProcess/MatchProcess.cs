using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using HexCore.GameMatch.View;
using ManagerBack.Hubs;
using Shared;
using Utility;

// TODO add games with password

namespace ManagerBack.Services;

public enum MatchStatus {
    WAITING_FOR_PLAYERS,
    READY_TO_START,
    IN_PROGRESS,
    FINISHED,
    CRASHED
}
public class MatchProcess {
    private static readonly Dictionary<BotType, string> BOT_TYPE_PATH_MAP = new() {
        {BotType.RANDOM, "../bots/random.lua"},
        {BotType.SMART, "../bots/basic.lua"},
    };

    public MatchStatus Status { get; private set; } = MatchStatus.WAITING_FOR_PLAYERS;
    public string CreatorId { get; }
    public Guid Id { get; }
    public QueuedPlayer?[] QueuedPlayers { get; }
    public MatchProcessConfig Config { get; }

    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public Match? Match { get; private set; } = null;
    public ConnectedMatchView View { get; }
    public MatchRecord Record { get; }
    public int TcpPort { get; }

    public TcpListener TcpListener { get; } 

    private readonly IMatchService _matchService;
    private readonly MatchConfig _matchConfig;
    private readonly ICardMaster _cardMaster;

    public MatchProcess(string creatorId, MatchProcessConfig config, MatchConfig mConfig, ICardMaster cardMaster, IMatchService matchService)
    {
        CreatorId = creatorId;
        Id = Guid.NewGuid();
        Config = config;

        // TODO for now this is a 2-player game, might change in the future
        QueuedPlayers = new QueuedPlayer[2];

        View = new ConnectedMatchView(Id, matchService);

        _matchService = matchService;
        _matchConfig = mConfig;
        _cardMaster = cardMaster;

        Record = new() {
            Config = config
        };

        TcpListener = new TcpListener(IPAddress.Loopback, 0);
        TcpListener.Start();
        TcpPort = ((IPEndPoint)TcpListener.LocalEndpoint).Port;
    }

    public Task InitialSetup() {
        for (int i = 0; i < QueuedPlayers.Length; i++) {
            // TODO change if ever will be more than 2 players
            var pConfig = i == 0 ? Config.P1Config : Config.P2Config;

            if (pConfig.BotConfig is null) {
                QueuedPlayers[i] = null;
                continue;
            }
            var controller = new LuaPlayerController(BOT_TYPE_PATH_MAP[pConfig.BotConfig!.BotType], pConfig.BotConfig.ActionDelay);

            var name = pConfig.BotConfig.Name;
            var deck = pConfig.BotConfig.StrDeck;

            var player = new QueuedPlayer(controller, new BotConnectionChecker(), true){
                Name = name,
                Deck = deck,
                Status = QueuedPlayerStatus.READY
            };

            player.StatusUpdated += OnPlayerStatusUpdated;
            SetQueuedPlayer(i, player);
        }

        if (CanConnect()) {
            Task.Run(ConnectTcpPlayers);
            // ConnectTcpPlayers();
        } else {
            Task.Run(TryRun);
        }

        return Task.CompletedTask;
    }

    private void SetQueuedPlayer(int idx, QueuedPlayer player) {
        if (QueuedPlayers[idx] is not null) {
            // TODO format like an actual warning
            Console.WriteLine($"WARNING: Setting SetQueuedPlayer with index {idx}, which is already set (matchId: {Id})");
        }

        QueuedPlayers[idx] = player;
    }

    public bool CanConnect() {
        return Status == MatchStatus.WAITING_FOR_PLAYERS && QueuedPlayers.Any(p => p is null);
    }

    private bool CanStart() {
        foreach (var player in QueuedPlayers) {
            if (player is null) return false;
            if (player.Status != QueuedPlayerStatus.READY) return false;
        }
        return true;
    }

    public async Task OnPlayerStatusUpdated() {
        await TryRun();
    }

    private async Task<bool> CheckPlayers() {
        var result = true;
        for (int i = 0; i < QueuedPlayers.Length; i++) {
            var player = QueuedPlayers[i];

            // * this is not needed, just for precaution
            if (player is null) {
                result = false;
                continue;
            }

            var valid = await player.Checker.Check();
            if (!valid) {
                result = false;
                QueuedPlayers[i] = null;
            }
        }
        return result;
    }

    public async Task TryRun() {
        if (!CanStart()) return;
        var valid = await CheckPlayers();
        if (!valid) {
            System.Console.WriteLine("some connections were invalid, waiting for new connections..");
            return;
        }
        System.Console.WriteLine("All players passed check, starting!");

        _ = Run();
    }

    private async Task CreatePlayerControllers() {
        foreach (var player in QueuedPlayers) {
            var baseController = player!.Controller;
            // TODO wrap with recording player controller
            var record = new PlayerRecord() {
                Name = player.Name!
            };
            Record.Players.Add(record);

            var controller = new RecordingPlayerController(baseController, record);

            await Match!.AddPlayer(player.GetName(), player.GetDeck(), controller!);
        }
    }

    private async Task SetStatus(MatchStatus status) {
        Status = status;
        await _matchService.ServiceStatusUpdated(this);
    }
    
    private async Task Run() {
        // * just in case
        if (Status >= MatchStatus.IN_PROGRESS) return;
        Match = new(Id.ToString(), _matchConfig, _cardMaster){
            View = View
        };

        // TODO replace with a db call
        Match.InitialSetup("../HexCore/core.lua");        
        
        await CreatePlayerControllers();

        System.Console.WriteLine("Match started!");
        await SetStatus(MatchStatus.IN_PROGRESS);
        StartTime = DateTime.Now;
        try {
            await Match.Start();
            await SetStatus(MatchStatus.FINISHED);
            Record.WinnerName = Match.Winner!.Name;
            await _matchService.ServiceStatusUpdated(this);
            System.Console.WriteLine("Match ended");
        } catch (Exception e) {
            await SetStatus(MatchStatus.CRASHED);
            Record.ExceptionMessage = e.Message;
            if (e.InnerException is not null)
                Record.InnerExceptionMessage = e.InnerException.Message;      
            // System.Console.WriteLine(e.Message);      
            // System.Console.WriteLine(e.StackTrace);
            System.Console.WriteLine("Match crashed");
            await Match.View.End();
        }

        EndTime = DateTime.Now; 
        // TcpListener.Stop();
    }

    public async Task AddWebSocketConnection(WebSocket socket) {
        var controller = new WebSocketPlayerController(socket);

        await AddPlayer(controller, new WebSocketConnectionChecker(socket));
    }

    private readonly object _addPlayerLock = new();
    private async Task AddPlayer(IOPlayerController controller, IConnectionChecker checker) {
        var player = new QueuedPlayer(controller, checker, false);
        player.StatusUpdated += OnPlayerStatusUpdated;
        player.Status = QueuedPlayerStatus.WAITING_FOR_DATA;

        var valid = await player.ReadPlayerInfo(checker);
        if (!valid) {
            return;
        }

        lock (_addPlayerLock) {
            var idx = GetFreeIdx();
            SetQueuedPlayer(idx, player);
        }

        player.Status = QueuedPlayerStatus.READY;
    }

    private int GetFreeIdx() {
        for (int i = 0; i < QueuedPlayers.Length; i++) {
            if (QueuedPlayers[i] is null) return i;
        }
        return -1;
    }

    public async Task Finish(WebSocket socket) {
        while (Status <= MatchStatus.IN_PROGRESS && socket.State == WebSocketState.Open) {
            await Task.Delay(200);
        }
    }

    private async Task ConnectTcpPlayers() {
        while (CanConnect()) {
            await AddTcpConnection();
        }
    }

    private async Task AddTcpConnection() {
        System.Console.WriteLine("Waiting for tcp connection...");
        var client = TcpListener.AcceptTcpClient();
        if (!CanConnect()) {
            client.Close();
            return;
        }
        System.Console.WriteLine("Connected!");
        var controller = new TCPPlayerController(client, Match!);
        await AddPlayer(controller, new TcpConnectionChecker(client));
        System.Console.WriteLine("player added!");
    }    
}



// public class MatchProcess_old {
//     public async Task ConnectTcpPlayers() {
//         // TODO keeps listening even after the match starts
//         while (CanAddConnection() && !Started()) {
//             await AddTCPConnection();
//         }
//     }

//     private Task<RecordingPlayerController> CreateRecordedPlayer(string name, IPlayerController baseController) {
//         var record = new PlayerRecord() {
//             Name = name
//         };
//         Record.Players.Add(record);
//         var result = new RecordingPlayerController(baseController, record);
//         return Task.FromResult(result);
//     }

//     private async Task<DeckTemplate> LoadDeck(string deck) {
//         var result = DeckTemplate.FromText(deck);
//         await _deckValidator.Validate(result);
//         return result;
//     }

//     public bool CanAddConnection() {
//         return Status == MatchStatus.WAITING_FOR_PLAYERS && _realPlayerCount > 0;
//     }

//     public async Task AddTCPConnection() {
//         var client = TcpListener.AcceptTcpClient();
//         if (!CanAddConnection()) {
//             client.Close();
//             return;
//         }
        
//         client.ReceiveTimeout = 1000;
//         var baseController = new TCPPlayerController(client, Match);

//         await AddPlayer(baseController);
//         client.ReceiveTimeout = 0;

//         TryRun();
//     }

//     private async Task AddPlayer(IOPlayerController baseController) {
//         string name;
//         string deckRaw;
//         try {
//             // TODO change to username extracted from jwt
//             await baseController.Write("name");
//             name = await baseController.Read();

//             // TODO this allows any user to submit any deck, change this later to deckId
//             await baseController.Write("deck");
//             deckRaw = await baseController.Read();
//         } catch (Exception e) {
//             Console.WriteLine(e.Message);
//             Console.WriteLine(e.StackTrace);
//             Console.WriteLine("Failed to connect");
//             return;
//         }
//         // TODO add exception handling
//         var deck = await LoadDeck(deckRaw);

//         var controller = await CreateRecordedPlayer(name, baseController);
//         await Match.AddPlayer(name, deck, controller);

//         --_realPlayerCount;
//     }

// }