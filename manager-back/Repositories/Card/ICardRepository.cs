namespace ManagerBack.Repositories;

public interface ICardRepository {
    public Task<CardModel?> ByCID(string cid);
    public Task Add(CardModel card);
    public Task<long> Delete(string cid);
    public Task<IEnumerable<CardModel>> All();
}