using System.Linq.Expressions;

namespace ManagerBack.Repositories;

public interface IDeckRepository {
    public Task<IEnumerable<DeckModel>> Filter(Expression<Func<DeckModel, bool>> filter);
    public Task Add(DeckModel deck);
    public Task<DeckModel?> ById(string id);
    public Task<long> Delete(string id);
    public Task<long> Update(string deckId, DeckModel update);
}