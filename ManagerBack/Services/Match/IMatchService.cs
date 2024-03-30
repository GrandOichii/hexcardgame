using System.Net.WebSockets;
using HexCore.GameMatch.States;

namespace ManagerBack.Services;

public interface IMatchService {
    public Task<MatchProcess> Create(string userId, MatchProcessConfig config);
    public Task WSConnect(WebSocketManager manager, string userId, string matchId);
    public Task<IEnumerable<GetMatchProcessDto>> All();
    public Task<GetMatchProcessDto> ById(string matchId);
    public Task ServiceStatusUpdated(MatchProcess match);
    public Task UpdateView(string matchId, BaseMatchState state);
    public Task EndView(string matchId);
    public Task SendMatchInfo(string matchId, string connectionId);
    public Task SendMatchState(string matchId, string connectionId);
    public Task Remove(Func<MatchProcess, bool> filter);

}