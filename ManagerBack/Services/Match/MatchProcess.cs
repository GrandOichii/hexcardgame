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

// TODO move
public enum QueuedPlayerStatus {
    WAITING_FOR_DATA,
    READY
}

public interface IConnectionChecker {
    public Task<string> Read();
    public Task<bool> Check();
}

public class BotConnectionChecker : IConnectionChecker
{
    public Task<bool> Check()
    {
        return Task.FromResult(true);
    }

    public Task<string> Read()
    {
        // * not needed, as bots are always ready
        return Task.FromResult("");
    }
}

public class WebSocketConnectionChecker : IConnectionChecker
{
    private readonly WebSocket _socket;

    public WebSocketConnectionChecker(WebSocket socket)
    {
        _socket = socket;
    }

    public async Task<bool> Check()
    {
        try {
            await _socket.Write("ping");
            var resp = await _socket.Read();
            return resp == "pong";
        } catch {
            // TODO bee more specific with exception types
            return false;
        }
    }

    public async Task<string> Read()
    {
        return await _socket.Read();
    }
}

public class TcpConnectionChecker : IConnectionChecker {
    private readonly TcpClient _socket;

    public TcpConnectionChecker(TcpClient socket)
    {
        _socket = socket;
    }

    public Task<bool> Check()
    {
        // TODO something similar to WebSocketConnectionChecker
        throw new NotImplementedException();
    }

    public Task<string> Read()
    {
        return Task.FromResult(
            NetUtil.Read(_socket.GetStream())
        );
    }
}

public class PlayerInfo {
    public required string Name { get; set; }
    public required string Deck { get; set; }
}

public class QueuedPlayer {
    
    public delegate Task StatusUpdate();
    public event StatusUpdate? StatusUpdated;
    private QueuedPlayerStatus _status = QueuedPlayerStatus.WAITING_FOR_DATA;
    public QueuedPlayerStatus Status {
        get => _status;
        set {
            _status = value;
            StatusUpdated?.Invoke();
        }
    }

    [JsonIgnore]
    public IPlayerController Controller { get; }

    // public PlayerConfig Config { get; }
    public string? Name { get; set; } = null;
    public string? Deck { get; set; } = null;
    
    [JsonIgnore]
    public IConnectionChecker Checker { get; }

    public QueuedPlayer(IPlayerController controller, IConnectionChecker checker)
    {
        Controller = controller;
        Checker = checker;
    }

    public string GetName() {
        return Name!;
    }

    public DeckTemplate GetDeck() {
        return DeckTemplate.FromText(Deck!);
    }

    public async Task<bool> ReadPlayerInfo(IConnectionChecker checker) {
        try {
            var data = await checker.Read();
            var info = JsonSerializer.Deserialize<PlayerInfo>(data, Common.JSON_SERIALIZATION_OPTIONS);

            // * failed to read data, consider the connection to be invalid
            if (info is null) {
                return false;
            }

            Name = info.Name;
            Deck = info.Deck;
        } catch (Exception e) {
            System.Console.WriteLine(e.Message);
            System.Console.WriteLine(e.StackTrace);
            return false;
        }
        return true;
    }
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

            var player = new QueuedPlayer(controller, new BotConnectionChecker()){
                Name = name,
                Deck = deck,
                Status = QueuedPlayerStatus.READY
            };

            player.StatusUpdated += OnPlayerStatusUpdated;
            SetQueuedPlayer(i, player);
        }

        _ = TryRun();

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
        return QueuedPlayers.Any(p => p is null);
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
        System.Console.WriteLine("Attempted to start match, checking players");
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
    
    private readonly object _runLock = new();
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
        var player = new QueuedPlayer(controller, checker);
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
}



// public class MatchProcess_old {
//     public string TcpAddress { get; set; }
//     [JsonIgnore]
//     public TcpListener TcpListener { get; } 
//     [JsonIgnore]
//     public Match Match { get; }
//     [JsonIgnore]
//     public MatchProcessConfig Config { get; }

//     private readonly IMatchService _matchService;
//     private readonly IValidator<DeckTemplate> _deckValidator;

//     public MatchProcess_old(IMatchService matchService, ICardMaster cMaster, MatchProcessConfig config, MatchConfig mConfig, IValidator<DeckTemplate> deckValidator)
//     {

//         TcpListener = new TcpListener(IPAddress.Loopback, 0);
//         TcpListener.Start();
//         TcpAddress = ((IPEndPoint)TcpListener.LocalEndpoint).ToString();


//         _deckValidator = deckValidator;

//         
//     }

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

//     public bool Started() {
//         return Status != MatchStatus.WAITING_FOR_PLAYERS;
//     }

//     private void TryRun() {
//         if (CanAddConnection()) return;

//         Run();
//     }

//     private async Task Run() {
//         System.Console.WriteLine("Match started!");
//         await SetStatus(MatchStatus.IN_PROGRESS);
//         StartTime = DateTime.Now;
//         try {
//             await Match.Start();
//             await SetStatus(MatchStatus.FINISHED);
//             Record.WinnerName = Match.Winner!.Name;
//             await _matchService.ServiceStatusUpdated(this);
//             System.Console.WriteLine("Match ended");
//         } catch (Exception e) {
//             await SetStatus(MatchStatus.CRASHED);
//             Record.ExceptionMessage = e.Message;
//             if (e.InnerException is not null)
//                 Record.InnerExceptionMessage = e.InnerException.Message;      
//             // System.Console.WriteLine(e.Message);      
//             // System.Console.WriteLine(e.StackTrace);
//             // System.Console.WriteLine("Match crashed");
//             await Match.View.End();
//         }

//         EndTime = DateTime.Now; 
//         TcpListener.Stop();
//     }
    
// }