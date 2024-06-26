using System.Linq.Expressions;

namespace ManagerBack.Tests.Mocks;

public class FakeUserRepository : IUserRepository
{
    public List<User> Users { get; set; } = new();

    public Task Add(User user)
    {
        if (user.Username.StartsWith("admin")) user.IsAdmin = true;
        Users.Add(user);
        user.Id = Users.Count.ToString();
        return Task.CompletedTask;
    }

    public Task<User?> ByUsername(string username)
    {
        return Task.FromResult(Users.FirstOrDefault(u => u.Username == username));
    }

    public Task<bool> CheckId(string id) {
        return Task.FromResult(
            Users.Count(u => u.Id == id) == 1
        );
    }
}
