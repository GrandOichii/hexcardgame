namespace Core.GameMatch.Phases;

/// <summary>
/// Match phase
/// </summary>
interface IMatchPhase {
    public void Exec(Match match, Player player);
}

