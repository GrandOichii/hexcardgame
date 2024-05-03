using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Claims;
using Shared;

namespace ManagerBack.Services;

/// <summary>
/// The exception that is thrown when failing to fetch the core file 
/// </summary>
[System.Serializable]
public class CoreFileNotFoundException : System.Exception
{
    public CoreFileNotFoundException() { }
    public CoreFileNotFoundException(string message) : base(message) { }
}

/// <summary>
/// Match process status
/// </summary>
public enum MatchStatus {
    /// <summary>
    /// Waiting for players to connect
    /// </summary>
    WAITING_FOR_PLAYERS,

    /// <summary>
    /// Ready to start
    /// </summary>
    READY_TO_START,

    /// <summary>
    /// Match process is in progress
    /// </summary>
    IN_PROGRESS,

    /// <summary>
    /// Match process is finished without error
    /// </summary>
    FINISHED,

    /// <summary>
    /// Match process crashed during running
    /// </summary>
    CRASHED
}


/// <summary>
/// Match process
/// </summary>
public class MatchProcess {
    /// <summary>
    /// Match seed generator
    /// </summary>
    private static readonly Random _seedGenerator = new();

    // TODO change to a db call
    private static readonly Dictionary<BotType, string> BOT_TYPE_PATH_MAP = new() {
        {BotType.RANDOM, "../bots/random.lua"},
        {BotType.SMART, "../bots/basic.lua"},
    };

    /// <summary>
    /// Match process changed delegate
    /// </summary>
    /// <param name="matchId">Match ID</param>
    public delegate Task MatchProcessChanged(string matchId);

    /// <summary>
    /// Match process changed event
    /// </summary>
    public event MatchProcessChanged? Changed;

    /// <summary>
    /// Match status
    /// </summary>
    public MatchStatus Status { get; private set; } = MatchStatus.WAITING_FOR_PLAYERS;

    /// <summary>
    /// User ID of the match creator
    /// </summary>
    public string CreatorId { get; }

    /// <summary>
    /// Match ID
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Array of the participants
    /// </summary>
    public QueuedPlayer?[] QueuedPlayers { get; }

    /// <summary>
    /// Match process configuration
    /// </summary>
    public MatchProcessConfig Config { get; }

    /// <summary>
    /// Match start time
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// Match end time
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Match object
    /// </summary>
    public Match? Match { get; private set; } = null;

    /// <summary>
    /// Match view
    /// </summary>
    public ConnectedMatchView View { get; }

    /// <summary>
    /// Match record
    /// </summary>
    public MatchRecord Record { get; }

    /// <summary>
    /// TCP port for connecting to the match
    /// </summary>
    public int TcpPort { get; }

    /// <summary>
    /// TCP listener
    /// </summary>
    public TcpListener TcpListener { get; } 

    /// <summary>
    /// Password hash
    /// </summary>
    private readonly string _passHash = "";


    /// <summary>
    /// Match service
    /// </summary>
    private readonly IMatchService _matchService;

    /// <summary>
    /// Match configuration
    /// </summary>
    private readonly MatchConfig _matchConfig;

    /// <summary>
    /// Card master 
    /// </summary>
    private readonly ICardMaster _cardMaster;

    /// <summary>
    /// Match scripts repository
    /// </summary>
    private readonly IMatchScriptsRepository _scriptsRepo;

    public MatchProcess(string creatorId, MatchProcessConfig config, MatchConfig mConfig, ICardMaster cardMaster, IMatchService matchService, IMatchScriptsRepository scriptsRepo)
    {
        if (!string.IsNullOrEmpty(config.Password))
            _passHash = BCrypt.Net.BCrypt.HashPassword(config.Password);

        CreatorId = creatorId;
        Id = Guid.NewGuid();
        Config = config;

        // TODO for now this is a 2-player game, might change in the future
        QueuedPlayers = new QueuedPlayer[2];

        View = new ConnectedMatchView(Id, matchService);

        _matchService = matchService;
        _matchConfig = mConfig;
        _cardMaster = cardMaster;

        Record = new()
        {
            Config = config,
            Seed = _seedGenerator.Next(),
            ConfigId = config.MatchConfigId
        };

        TcpListener = new TcpListener(IPAddress.Loopback, 0);
        TcpListener.Start();
        TcpPort = ((IPEndPoint)TcpListener.LocalEndpoint).Port;
        _scriptsRepo = scriptsRepo;
    }

    /// <summary>
    /// Performs the initial match sertup
    /// </summary>
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
            player.Changed += OnPlayerChanged;

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

    /// <summary>
    /// Event handler for player changing event
    /// </summary>
    public async Task OnPlayerChanged() {
        if (Changed is not null)
            await Changed.Invoke(Id.ToString());
    }

    /// <summary>
    /// Sets the queued player to the specified index
    /// </summary>
    /// <param name="idx">Queued player index</param>
    /// <param name="player">Queued player</param>
    private void SetQueuedPlayer(int idx, QueuedPlayer player) {
        if (QueuedPlayers[idx] is not null) {
            Console.WriteLine($"WARNING: Setting SetQueuedPlayer with index {idx}, which is already set (matchId: {Id})");
        }

        QueuedPlayers[idx] = player;
    }

    /// <summary>
    /// Checks whether another player can connect to the match
    /// </summary>
    /// <returns>True if can connect, else false</returns>
    public bool CanConnect() {
        return Status == MatchStatus.WAITING_FOR_PLAYERS && QueuedPlayers.Any(p => p is null);
    }

    /// <summary>
    /// Checks whether the match can start
    /// </summary>
    /// <returns>True if can start, else false</returns>
    private bool CanStart() {
        foreach (var player in QueuedPlayers) {
            if (player is null) return false;
            if (player.Status != QueuedPlayerStatus.READY) return false;
        }
        return true;
    }

    /// <summary>
    /// Event handler for player status changing
    /// </summary>
    public async Task OnPlayerStatusUpdated() {
        await TryRun();
    }

    /// <summary>
    /// Checks the connection of all of the players, all failed connections are removed
    /// </summary>
    /// <returns>True if all connectios are valid, else false</returns>
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

    /// <summary>
    /// Attempts to run the match
    /// </summary>
    public async Task TryRun() {
        if (!CanStart()) return;
        var valid = await CheckPlayers();
        if (!valid) {
            return;
        }

        _ = Run();
    }

    /// <summary>
    /// Wraps all of the player controllers in separate RecordingPlayerController instances, adds them to the match object
    /// </summary>
    private async Task CreatePlayerControllers() {
        foreach (var player in QueuedPlayers) {
            var baseController = player!.Controller;
            var record = new PlayerRecord() {
                Name = player.Name!,
                Deck = player.Deck!,
            };
            Record.Players.Add(record);

            var controller = new RecordingPlayerController(baseController, record);

            await Match!.AddPlayer(player.GetName(), player.GetDeck(), controller!);
        }
    }

    /// <summary>
    /// Updates the status of the match process
    /// </summary>
    /// <param name="status">Match status</param>
    private async Task SetStatus(MatchStatus status) {
        Status = status;
        await _matchService.ServiceStatusUpdated(this);
        
        if (Changed is not null)
            await Changed.Invoke(Id.ToString());
    }

    /// <summary>
    /// Runs the match
    /// </summary>
    private async Task Run() {
        // * just in case
        if (Status >= MatchStatus.IN_PROGRESS) return;
        Match = new(Id.ToString(), _matchConfig, _cardMaster, Record.Seed ){
            View = View,
        };

        System.Console.WriteLine("fetching");

        var script = await _scriptsRepo.GetCoreScript() ??
            throw new CoreFileNotFoundException()
        ;

        System.Console.WriteLine("fetched " + script.Script);
        
        Match.InitialSetup(script.Script);
        
        await CreatePlayerControllers();

        await SetStatus(MatchStatus.IN_PROGRESS);
        StartTime = DateTime.Now;
        try {
            await Match.Start();
            await SetStatus(MatchStatus.FINISHED);
            Record.WinnerName = Match.Winner!.Name;
            await _matchService.ServiceStatusUpdated(this);
        } catch (Exception e) {
            await SetStatus(MatchStatus.CRASHED);
            Record.ExceptionMessage = e.Message;
            if (e.InnerException is not null)
                Record.InnerExceptionMessage = e.InnerException.Message;      
            System.Console.WriteLine(e.Message);      
            System.Console.WriteLine(e.StackTrace);
            System.Console.WriteLine("Match crashed");
            await Match.View.End();
        }

        EndTime = DateTime.Now; 
    }

    /// <summary>
    /// Attempts to add a WebSocket connection
    /// </summary>
    /// <param name="socket">WebSocket connection</param>
    public async Task AddWebSocketConnection(WebSocket socket) {
        var controller = new WebSocketPlayerController(socket);

        await AddPlayer(controller, new WebSocketConnectionChecker(socket));
    }

    /// <summary>
    /// Lock object for adding players to the match
    /// </summary>
    private readonly object _addPlayerLock = new();

    /// <summary>
    /// Add a new player to the QueuedPlayers
    /// </summary>
    /// <param name="controller">Player controller</param>
    /// <param name="checker">Connection checker</param>
    private async Task AddPlayer(IOPlayerController controller, IConnectionChecker checker) {
        var player = new QueuedPlayer(controller, checker, false);
        player.Changed += OnPlayerChanged;

        player.StatusUpdated += OnPlayerStatusUpdated;
        player.Status = QueuedPlayerStatus.WAITING_FOR_DATA;

        var errMsg = await player.ReadPlayerInfo(this, checker);
        if (!string.IsNullOrEmpty(errMsg)) {
            await checker.Write(errMsg);
            return;
        }

        lock (_addPlayerLock) {
            var idx = GetFreeIdx();
            SetQueuedPlayer(idx, player);
        }
        await checker.Write("accept");

        player.Status = QueuedPlayerStatus.READY;
    }

    /// <summary>
    /// Retreives an empty index in the QueuedPlayers array
    /// </summary>
    /// <returns>A free index</returns>
    private int GetFreeIdx() {
        for (int i = 0; i < QueuedPlayers.Length; i++) {
            if (QueuedPlayers[i] is null) return i;
        }
        return -1;
    }

    /// <summary>
    /// Runs a loop for a WebSocket connection to keep it from disconnecting until the match is finished
    /// </summary>
    /// <param name="socket">WebSocket connection</param>
    public async Task Finish(WebSocket socket) {
        while (Status <= MatchStatus.IN_PROGRESS && socket.State == WebSocketState.Open) {
            await Task.Delay(200);
        }
    }

    /// <summary>
    /// Runs a loop that will attempt to add TCP connections
    /// </summary>
    private async Task ConnectTcpPlayers() {
        while (CanConnect()) {
            await AddTcpConnection();
        }
    }

    /// <summary>
    /// Add a signle TCP connection
    /// </summary>
    private async Task AddTcpConnection() {
        var client = TcpListener.AcceptTcpClient();
        if (!CanConnect()) {
            client.Close();
            return;
        }
        var jwt = NetUtil.Read(client.GetStream());
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(jwt);
        if (jsonToken is not JwtSecurityToken tokenS) return;

        var userId = tokenS.Claims.First(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
        System.Console.WriteLine(userId);

        var controller = new TCPPlayerController(client, Match!);
        await AddPlayer(controller, new TcpConnectionChecker(client));
    }

    /// <summary>
    /// Checks the match password
    /// </summary>
    /// <param name="password">Potential password</param>
    /// <returns>True if the potential password matches the match password, else false</returns>
    public bool CheckPassword(string password) {
        if (!RequiresPassword()) return true;

        return BCrypt.Net.BCrypt.Verify(password, _passHash);
    }

    /// <summary>
    /// Check whether the match requires a password
    /// </summary>
    /// <returns>True is password is required, else false</returns>
    public bool RequiresPassword() => !string.IsNullOrEmpty(_passHash);
}