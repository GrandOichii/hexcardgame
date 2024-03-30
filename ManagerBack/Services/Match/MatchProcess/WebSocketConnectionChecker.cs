using System.Net.WebSockets;

namespace ManagerBack.Services;

public class WebSocketConnectionChecker : IConnectionChecker
{
    private readonly WebSocket _socket;

    public WebSocketConnectionChecker(WebSocket socket)
    {
        _socket = socket;
    }

    public async Task<bool> Check()
    {
        try {
            await _socket.Write("ping");
            var resp = await _socket.Read();
            return resp == "pong";
        } catch {
            // TODO bee more specific with exception types
            return false;
        }
    }

    public async Task<string> Read()
    {
        return await _socket.Read();
    }
}
