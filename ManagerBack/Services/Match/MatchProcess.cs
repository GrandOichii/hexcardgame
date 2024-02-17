using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

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
    public Guid Id { get; }

    private readonly Match _match;
    private int _realPlayerCount;

    private static readonly Dictionary<BotType, string> BOT_TYPE_PATH_MAP = new() {
        {BotType.RANDOM, "../bots/random.lua"},
        {BotType.SMART, "../bots/basic.lua"},
    };

    public MatchProcess(CardMaster cMaster, MatchProcessConfig config)
    {
        Id = Guid.NewGuid();

        _match = new Match(Id.ToString(), config.MatchConfig, cMaster, "../core/core.lua");
        // TODO fix the order of the players

        _realPlayerCount = 0;
        // add bot players
        foreach (var p in new List<PlayerConfig> {config.P1Config, config.P2Config}) {
            if (p.BotConfig is null) {
                ++_realPlayerCount;
                continue;
            }

            var controller = new LuaPlayerController(BOT_TYPE_PATH_MAP[p.BotConfig.BotType]);
            var deck = DeckTemplate.FromText(p.BotConfig.StrDeck);
            // TODO validate deck
            var player = new Player(_match, p.BotConfig.Name, deck, controller);
        }
    }

    public bool CanAddConnection() {
        return Status == MatchStatus.WAITING_FOR_PLAYERS && _realPlayerCount > 0;
    }

    public async Task AddWebSocketConnection(WebSocket socket) {
        // TODO change to username
        await socket.Write("name");
        string name = await socket.Read();

        // TODO this allows any user to submit any deck, change this later to deckId
        await socket.Write("deck");
        var resp = await socket.Read();
        var deck = DeckTemplate.FromText(resp);
        // TODO validate deck

        var player = new Player(_match, name, deck, new WebSocketPlayerController(socket));
        --_realPlayerCount;
        if (CanAddConnection()) return;

        Run();
    }

    public bool Started() {
        return Status != MatchStatus.WAITING_FOR_PLAYERS;
    }

    private async Task Run() {
        Status = MatchStatus.IN_PROGRESS;
        Record = new();

        try {
            _match.Start();
            Status = MatchStatus.FINISHED;
        } catch (Exception e) {
            Status = MatchStatus.CRASHED;
            Record.ExceptionMessage = e.Message;
            if (e.InnerException is not null)
                Record.InnerExceptionMessage = e.InnerException.Message;
            Console.WriteLine(e);
        }
    }
    
    public async Task Finish() {
        // TODO this seems wrong
        while (Status == MatchStatus.IN_PROGRESS) {
            await Task.Delay(200);
        }
    }

}