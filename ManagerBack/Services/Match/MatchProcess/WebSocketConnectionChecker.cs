using System.Net.WebSockets;

namespace ManagerBack.Services;

/// <summary>
/// Checker for a WebSocket connection
/// </summary>
public class WebSocketConnectionChecker : IConnectionChecker
{
    /// <summary>
    /// Connection socket
    /// </summary>
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

    public async Task Write(string msg)
    {
        await _socket.Write(msg);
    }
}
