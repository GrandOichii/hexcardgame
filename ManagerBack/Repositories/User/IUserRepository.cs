namespace ManagerBack.Repositories;

public interface IUserRepository {
    public Task Add(User user);
    public Task<User?> ByUsername(string username);
    public Task<bool> CheckId(string id);
}