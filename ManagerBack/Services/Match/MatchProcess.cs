using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using HexCore.GameMatch.View;
using ManagerBack.Hubs;
using Microsoft.VisualBasic;

// TODO add games with password

namespace ManagerBack.Services;

public enum MatchStatus {
    WAITING_FOR_PLAYERS,
    IN_PROGRESS,
    FINISHED,
    CRASHED
}

public class MatchProcess {
    public MatchStatus Status { get; private set; } = MatchStatus.WAITING_FOR_PLAYERS;
    public MatchRecord Record { get; }
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
    private readonly IValidator<DeckTemplate> _deckValidator;
    public ConnectedMatchView View { get; }

    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public MatchProcess(IMatchService matchService, ICardMaster cMaster, MatchProcessConfig config, MatchConfig mConfig, IValidator<DeckTemplate> deckValidator)
    {
        _matchService = matchService;
        _config = config;
        Id = Guid.NewGuid();

        TcpListener = new TcpListener(IPAddress.Loopback, 0);
        TcpListener.Start();
        TcpAddress = ((IPEndPoint)TcpListener.LocalEndpoint).ToString();


        _match = new Match(Id.ToString(), mConfig, cMaster);
        View = new ConnectedMatchView(Id, matchService);
        _match.View = View;

        _match.InitialSetup("../HexCore/core.lua");
        // TODO fix the order of the players

        _realPlayerCount = 0;
        _deckValidator = deckValidator;

        Record = new() {
            Config = config
        };
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

    private Task<RecordingPlayerController> CreateRecordedPlayer(string name, IPlayerController baseController) {
        var record = new PlayerRecord() {
            Name = name
        };
        Record.Players.Add(record);
        var result = new RecordingPlayerController(baseController, record);
        return Task.FromResult(result);
    }

    public async Task AddBots() {
        foreach (var p in new List<PlayerConfig> {_config.P1Config, _config.P2Config}) {
            if (p.BotConfig is null) {
                ++_realPlayerCount;
                continue;
            }

            var controller = await CreateRecordedPlayer(p.BotConfig.Name, new LuaPlayerController(BOT_TYPE_PATH_MAP[p.BotConfig.BotType]));
            var deck = await LoadDeck(p.BotConfig.StrDeck);
            await _match.AddPlayer(p.BotConfig.Name, deck, controller);
        }
        if (!CanAddConnection()) {
            Run();
            return;
        }
        Task.Run(ConnectTcpPlayers);
    }

    private async Task<DeckTemplate> LoadDeck(string deck) {
        var result = DeckTemplate.FromText(deck);
        await _deckValidator.Validate(result);
        return result;
    }

    public bool CanAddConnection() {
        return Status == MatchStatus.WAITING_FOR_PLAYERS && _realPlayerCount > 0;
    }

    public async Task AddWebSocketConnection(WebSocket socket) {
        var controller = new WebSocketPlayerController(socket);

        await AddPlayer(controller);
    }

    public async Task AddTCPConnection() {
        // var task = TcpListener.AcceptTcpClientAsync();
        // var success = task.Wait(100);
        // if (!success) {
        //     return;
        // }
        // var client = task.Result;
        var client = TcpListener.AcceptTcpClient();
        var baseController = new TCPPlayerController(client, _match);

        await AddPlayer(baseController);
    }

    private async Task AddPlayer(IOPlayerController baseController)  {
        // TODO change to username extracted from jwt
        await baseController.Write("name");
        var name = await baseController.Read();

        // TODO this allows any user to submit any deck, change this later to deckId
        await baseController.Write("deck");
        var deckRaw = await baseController.Read();
        var deck = await LoadDeck(deckRaw);

        var controller = await CreateRecordedPlayer(name, baseController);
        await _match.AddPlayer(name, deck, controller);

        --_realPlayerCount;
        if (CanAddConnection()) return;

        Run();
    }

    public bool Started() {
        return Status != MatchStatus.WAITING_FOR_PLAYERS;
    }

    private async Task Run() {
        await SetStatus(MatchStatus.IN_PROGRESS);
        StartTime = DateTime.Now;
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
        EndTime = DateTime.Now; 
        TcpListener.Stop();
    }
    
    public async Task Finish() {
        // TODO this seems wrong
        while (Status == MatchStatus.IN_PROGRESS) {
            await Task.Delay(200);
        }
    }

}