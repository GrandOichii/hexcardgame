using System.Linq.Expressions;

namespace ManagerBack.Tests.Mocks;

public class MockUserRepository : IUserRepository
{
    public List<User> Users { get; set; } = new();

    public async Task Add(User user)
    {
        if (user.Username.StartsWith("admin")) user.IsAdmin = true;
        Users.Add(user);
        user.Id = Users.Count.ToString();
    }

    public async Task<User?> ByUsername(string username)
    {
        return Users.FirstOrDefault(u => u.Username == username);
    }
}
