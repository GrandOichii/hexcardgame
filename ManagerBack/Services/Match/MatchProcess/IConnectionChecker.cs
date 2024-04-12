namespace ManagerBack.Services;

public interface IConnectionChecker {
    public Task<string> Read();
    public Task Write(string msg);
    public Task<bool> Check();
}
