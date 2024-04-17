using System.Linq.Expressions;
using ManagerBack.Controllers;
using MongoDB.Driver;

namespace ManagerBack.Repositories;

public interface ICardRepository {
    public Task<CardModel?> ByCID(string cid);
    public Task Add(CardModel card);
    public Task<long> Delete(string cid);
    public Task<IEnumerable<CardModel>> All();
    public Task<IEnumerable<CardModel>> Filter(Expression<Func<CardModel, bool>> filter);
    public Task<long> Update(CardModel card);
    public Task<IEnumerable<CardModel>> Query(CardQuery query);
}