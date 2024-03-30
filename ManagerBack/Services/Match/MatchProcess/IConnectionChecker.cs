namespace ManagerBack.Services;

public interface IConnectionChecker {
    public Task<string> Read();
    public Task<bool> Check();
}
