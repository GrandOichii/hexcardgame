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
    public MatchStatus Status { get; } = MatchStatus.WAITING_FOR_PLAYERS;
    public Guid Id { get; }

    private readonly Match _match;

    public MatchProcess(CardMaster cMaster, MatchProcessConfig config)
    {
        Id = Guid.NewGuid();

        // _match = new Match(Id.ToString(), config.MatchConfig, cMaster);
    }
}