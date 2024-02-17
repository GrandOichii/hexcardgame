namespace Core.GameMatch.Actions;


/// <summary>
/// Action for executing match Lua commands
/// </summary>
class ExecuteCommandAction : IGameAction
{
    public void Exec(Match match, Player player, string[] args)
    {
        if (!match.AllowCommands) return;

        var command = "";
        for (int i = 1; i < args.Length; i++)
            command += args[i] + " ";
        match.LState.DoString(command);
    }
}
