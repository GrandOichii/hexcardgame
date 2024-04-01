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
            System.Console.WriteLine("WebSocket check, sent ping...");
            await _socket.Write("ping");
            
            var timeOut = new CancellationTokenSource(5000).Token;
            var resp = await _socket.Read(timeOut);
            System.Console.WriteLine("WebSocket received " + resp);
            return resp == "pong";
        // catch(OperationCancelledException)
        // {
        //     DoInactivityAction();
        // }
        } catch (Exception e) {
            System.Console.WriteLine(e.Message);
            // TODO bee more specific with exception types
            return false;
        }
    }

    public async Task<string> Read()
    {
        return await _socket.Read();
    }
}
