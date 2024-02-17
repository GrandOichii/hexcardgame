namespace Core.GameMatch.Actions;

/// <summary>
/// Game action, performed by the player
/// </summary>
interface IGameAction
{
    /// <summary>
    /// Executes the game action
    /// </summary>
    /// <param name="match">Game match</param>
    /// <param name="player">Player who executes the action</param>
    /// <param name="args">The arguments of the action</param>
    public void Exec(Match match, Player player, string[] args);
}
