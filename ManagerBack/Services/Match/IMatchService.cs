namespace ManagerBack.Services;



public interface IMatchService {
    public Task<MatchProcess> Create(MatchProcessConfig config);
    public Task Connect(string matchId);
    public Task<IEnumerable<MatchProcess>> All();
}