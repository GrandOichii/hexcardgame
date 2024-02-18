namespace HexCore.GameMatch.View;

/// <summary>
/// Empty match view, does nothing with the new information
/// </summary>
public class EmptyMatchView : IMatchView
{
    public Task Start()
    {
        return Task.CompletedTask;
    }

    public Task Update(Match match)
    {
        return Task.CompletedTask;
    }
    public Task End()
    {
        return Task.CompletedTask;
    }
}
