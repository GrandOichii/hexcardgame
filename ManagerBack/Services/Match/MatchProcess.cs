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

    public MatchProcess(CardMaster cMaster, MatchProcessConfig config)
    {
        Id = Guid.NewGuid();

        _match = new Match(Id.ToString(), config.MatchConfig, cMaster, "../core/core.lua");
        // add bot players
        foreach (var p in new List<PlayerConfig> {config.P1Config, config.P2Config}) {
            if (p.IsBot) continue;

            // TODO configure bot type
            var controller = new LuaPlayerController("../bots/random.lua");
            // TODO figure out how to add decks to bots
        }
        // TODO
    }

    public bool CanAddConnection() {
        // TODO
        return false;
    }

    public void AddConnection(WebSocket socket, string userId) {
        // TODO
    }

    public bool CanStart() {
        // TODO
        return false;
    }

    public async Task Start() {
        Status = MatchStatus.IN_PROGRESS;
        Record = new();

        try {
            _match.Start();
            Status = MatchStatus.FINISHED;
        } catch (Exception e) {
            Status = MatchStatus.CRASHED;
            Record.ExceptionMessage = e.Message;
        }
    }
    
    public async Task Finish() {
        // TODO this seems wrong
        while (Status == MatchStatus.IN_PROGRESS) {
            await Task.Delay(200);
        }
    }

}