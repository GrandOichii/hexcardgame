using System.Linq.Expressions;

namespace ManagerBack.Tests.Mocks;

public class FakeMatchConfigRepository : IMatchConfigRepository
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

    public Task<IEnumerable<MatchConfigModel>> Filter(Expression<Func<MatchConfigModel, bool>> filter)
    {
        var f = filter.Compile();
        return Task.FromResult(Configs.FindAll(f.Invoke).AsEnumerable());
    }
}
