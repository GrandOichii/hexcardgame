using ManagerBack.Controllers;

namespace ManagerBack.Services;

public interface ICardService {
    public Task<IEnumerable<string>> AllCIDs();    
    public Task<ExpansionCard> ByCID(string cid);
    public Task<ExpansionCard> Create(ExpansionCard card);
    public Task Update(ExpansionCard card);
    public Task Delete(string cid);
    public Task<IEnumerable<ExpansionCard>> All();
    public Task<IEnumerable<ExpansionCard>> Query(CardQuery query);
}