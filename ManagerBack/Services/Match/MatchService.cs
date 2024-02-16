

using System.Net.WebSockets;
using System.Text;

namespace ManagerBack.Services;

[Serializable]
public class MatchNotFoundException : Exception
{
    public MatchNotFoundException() { }
    public MatchNotFoundException(string matchId) : base($"no match with id {matchId}") { }
}

[Serializable]
public class MatchRefusedConnectionException : Exception
{
    public MatchRefusedConnectionException() { }
    public MatchRefusedConnectionException(string matchId) : base($"match with id {matchId} refused connection") { }
}

public class MatchService : IMatchService
{
    private readonly CardMaster _cardMaster;
    private readonly Dictionary<Guid, MatchProcess> _matches = new();
    public MatchService(ICardRepository cardRepo)
    {
        _cardMaster = new DBCardMaster(cardRepo);
    }

    #region IO
    
    private static async Task Write(WebSocket socket, string message) {
        var serverMsg = Encoding.UTF8.GetBytes(message);
        await socket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private static async Task<string> Read(WebSocket socket) {
        var buffer = new byte[1024 * 4];
        await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        return Encoding.UTF8.GetString(buffer).Replace("\0", string.Empty);
    }
            
    #endregion

    public async Task Connect(WebSocketManager manager, string userId, string matchId)
    {
        var guid = Guid.Parse(matchId);
        if (!_matches.ContainsKey(guid))
            throw new MatchNotFoundException(matchId);

        var match = _matches[guid];

        if (!match.CanAddConnection())
            throw new MatchRefusedConnectionException(matchId);

        
        var socket = await manager.AcceptWebSocketAsync();
        match.AddConnection(socket, userId);

        do {
            // TODO poll until can start match

        } while (!match.CanStart());

        // TODO send notice that match is starting and wait for response

        var record = match.Start();

        // TODO save record to db/cache
    }

    public async Task<MatchProcess> Create(string userId, MatchProcessConfig config)
    {
        var result = new MatchProcess(_cardMaster, config);
        _matches.Add(result.Id, result);
        return result;
    }

    public async Task<IEnumerable<MatchProcess>> All()
    {
        return _matches.Values;
    }
}