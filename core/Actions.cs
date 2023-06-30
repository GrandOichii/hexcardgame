using core.players;

namespace core.match;


/// <summary>
/// Game action, performed by the player
/// </summary>
abstract class GameAction
{
    /// <summary>
    /// Executes the game action
    /// </summary>
    /// <param name="match">Game match</param>
    /// <param name="player">Player who executes the action</param>
    /// <param name="args">The arguments of the action</param>
    abstract public void Exec(Match match, Player player, string[] args);
}


/// <summary>
/// Empty action, does nothing
/// </summary>
class DoNothingAction : GameAction
{
    public override void Exec(Match match, Player player, string[] args)
    {
    }
}

/// <summary>
/// Action for executing match Lua commands
/// </summary>
class ExecuteCommandAction : GameAction
{
    public override void Exec(Match match, Player player, string[] args)
    {
        if (!match.AllowCommands) return;

        var command = "";
        for (int i = 1; i < args.Length; i++)
            command += args[i] + " ";
        match.LState.DoString(command);
    }
}