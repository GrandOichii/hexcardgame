using System.Net.WebSockets;
using System.Text;

namespace ManagerBack.Services;

/// <summary>
/// Player controller, controlled by a WebSocket connection
/// </summary>
public class WebSocketPlayerController : IOPlayerController {
    private readonly WebSocket _socket;

    public WebSocketPlayerController(WebSocket socket)
    {
        _socket = socket;
    }

    public override async Task Write(string message)
    {
        await _socket.Write(message);
    }

    public override async Task<string> Read()
    {
        return await _socket.Read();
    }

    public override async Task CleanUp()
    {
        // TODO? more
        await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "matchend", CancellationToken.None);
    }
}
