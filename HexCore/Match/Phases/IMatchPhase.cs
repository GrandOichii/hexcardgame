namespace HexCore.GameMatch.Phases;

/// <summary>
/// Match phase
/// </summary>
interface IMatchPhase {
    public Task Exec(Match match, Player player);
}

