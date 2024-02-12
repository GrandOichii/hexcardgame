using ManagerBack.Dtos;

namespace ManagerBack.Services;

public interface IDeckService {
    public Task<IEnumerable<DeckModel>> All(string userId);
    public Task<DeckModel> Create(string userId, PostDeckDto deck);
    public Task Delete(string userId, string deckId);
    public Task<DeckModel> Update(string userId, string deckId, PostDeckDto deck);
}