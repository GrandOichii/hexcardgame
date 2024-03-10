namespace ManagerBack.Services;

public interface IMatchConfigService {
    public Task<IEnumerable<MatchConfigModel>> All();
    public Task<MatchConfigModel> Create(MatchConfig config);
    public Task<MatchConfigModel> ById(string id);
    public Task<MatchConfigModel> Basic();
}