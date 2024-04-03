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
            
            var timeOut = new CancellationTokenSource(5000).Token;
            var resp = await _socket.Read(timeOut);
            return resp == "pong";
        } catch (Exception e) {
            System.Console.WriteLine(e.Message);
            return false;
        }
    }

    public async Task<string> Read()
    {
        return await _socket.Read();
    }
}
