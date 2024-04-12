namespace ManagerBack.Services;

public class BotConnectionChecker : IConnectionChecker
{
    public Task<bool> Check()
    {
        return Task.FromResult(true);
    }

    public Task<string> Read()
    {
        // * not needed, as bots are always ready
        return Task.FromResult("");
    }

    public Task Write(string msg)
    {
        return Task.CompletedTask;
    }
}
