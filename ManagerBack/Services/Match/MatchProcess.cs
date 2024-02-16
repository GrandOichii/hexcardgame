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
    public Guid Id { get; }

    private readonly Match _match;

    public MatchProcess(CardMaster cMaster, MatchProcessConfig config)
    {
        Id = Guid.NewGuid();

        // TODO
        // _match = new Match(Id.ToString(), config.MatchConfig, cMaster);
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

    public MatchRecord Start() {
        // TODO
        var result = new MatchRecord();

        try {
            _match.Start();
            Status = MatchStatus.FINISHED;
        } catch (Exception e) {
            Status = MatchStatus.CRASHED;
            result.ExceptionMessage = e.Message;
        }

        return result;
    }

}