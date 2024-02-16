namespace ManagerBack.Tests.Mocks;

// TODO i dont think this is needed
public class MockCachedCardRepository : ICachedCardRepository {
    
    public async Task Forget(string cid)
    {
    }

    public async Task<CardModel?> Get(string cid)
    {
        return null;
    }

    public async Task Remember(CardModel card)
    {
    }   
}