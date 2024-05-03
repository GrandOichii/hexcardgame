using System.Net.WebSockets;
using HexCore.GameMatch.States;

namespace ManagerBack.Services;

/// <summary>
/// Match process service
/// </summary>
public interface IMatchService {
    /// <summary>
    /// Creates a new match process
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="config">Match process configuration</param>
    /// <returns>Created match information</returns>
    public Task<GetMatchProcessDto> Create(string userId, MatchProcessConfig config);

    /// <summary>
    /// Attempts a WebSocket connection
    /// </summary>
    /// <param name="manager">WebSocket manager</param>
    /// <param name="userId">User ID</param>
    /// <param name="matchId">Match process ID</param>
    public Task WSConnect(WebSocketManager manager, string userId, string matchId);

    /// <summary>
    /// Fetches all match processes
    /// </summary>
    /// <returns>Enumerable of all match processes</returns>
    public Task<IEnumerable<GetMatchProcessDto>> All();

    /// <summary>
    /// Fecthes a match process by ID
    /// </summary>
    /// <param name="matchId">Match process ID</param>
    /// <returns>The match process</returns>
    public Task<GetMatchProcessDto> ById(string matchId);

    /// <summary>
    /// Event handler for a service update event
    /// </summary>
    /// <param name="match">Match process</param>
    public Task ServiceStatusUpdated(MatchProcess match);

    /// <summary>
    /// Updates the view for all match viewers
    /// </summary>
    /// <param name="matchId">Match ID</param>
    /// <param name="state">Match state</param>
    public Task UpdateView(string matchId, BaseMatchState state);

    /// <summary>
    /// Ends the match process view
    /// </summary>
    /// <param name="matchId">Match ID</param>
    public Task EndView(string matchId);

    /// <summary>
    /// Sends the match process info to the connection
    /// </summary>
    /// <param name="matchId">Match ID</param>
    /// <param name="connectionId">Connection ID</param>
    public Task SendMatchInfo(string matchId, string connectionId);

    /// <summary>
    /// Sends the match state to the connection
    /// </summary>
    /// <param name="matchId">Match ID</param>
    /// <param name="connectionId">Connection ID</param>
    public Task SendMatchState(string matchId, string connectionId);

    /// <summary>
    /// Removes the matches using the filter function
    /// </summary>
    /// <param name="filter">Filter function</param>
    public Task Remove(Func<MatchProcess, bool> filter);

    /// <summary>
    /// Registers a match process watcher
    /// </summary>
    /// <param name="matchId">Match ID</param>
    /// <param name="connectionId">Connection ID</param>
    public Task RegisterWatcher(string matchId, string connectionId);
}