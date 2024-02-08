namespace ManagerBack.Services;

public interface ICardService {
    
    public Task<ExpansionCard> ByCID(string cid);
    public Task<ExpansionCard> Create(ExpansionCard card);
    public Task Delete(string cid);
    public Task<IEnumerable<ExpansionCard>> All();
}