using System.Net.WebSockets;

namespace ManagerBack.Services;



public interface IMatchService {
    public Task<MatchProcess> Create(string userId, MatchProcessConfig config);
    public Task Connect(WebSocketManager manager, string userId, string matchId);
    public Task<IEnumerable<MatchProcess>> All();
}