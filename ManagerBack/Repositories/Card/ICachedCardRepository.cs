namespace ManagerBack.Repositories;

public interface ICachedCardRepository {
    public Task Remember(CardModel card);
    public Task Forget(string cid);
    public Task<CardModel?> Get(string cid);
}