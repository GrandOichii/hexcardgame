using System.Linq.Expressions;

namespace ManagerBack.Tests.Mocks;

public class MockMatchConfigRepository : IMatchConfigRepository
{
    public List<MatchConfigModel> Configs { get; } = new();
    public Task Add(MatchConfigModel config)
    {
        Configs.Add(config);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<MatchConfigModel>> All()
    {
        return Task.FromResult(Configs.AsEnumerable());
    }

    public Task<MatchConfigModel?> ById(string id)
    {
        return Task.FromResult(Configs.FirstOrDefault(c => c.Id == id));
    }
}
