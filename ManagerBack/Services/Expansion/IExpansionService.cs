namespace ManagerBack.Services;

public interface IExpansionService {
    public Task<IEnumerable<Expansion>> All();
    public Task<Expansion> ByName(string expansion);
}