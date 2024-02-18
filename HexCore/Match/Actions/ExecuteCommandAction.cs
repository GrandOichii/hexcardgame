namespace HexCore.GameMatch.Actions;


/// <summary>
/// Action for executing match Lua commands
/// </summary>
class ExecuteCommandAction : IGameAction
{
    public Task Exec(Match match, Player player, string[] args)
    {
        if (!match.AllowCommands) return Task.CompletedTask;

        var command = "";
        for (int i = 1; i < args.Length; i++)
            command += args[i] + " ";
        match.LState.DoString(command);
        return Task.CompletedTask;
    }
}
