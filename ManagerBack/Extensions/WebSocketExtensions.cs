using System.Net.WebSockets;
using System.Text;

namespace ManagerBack.Extensions;

public static class WebSocketExtensions {
    public static async Task<string> Read (this WebSocket socket) {
        var buffer = new byte[1024 * 4];
        await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        return Encoding.UTF8.GetString(buffer).Replace("\0", string.Empty);
    }

    public static async Task Write(this WebSocket socket, string message) {
        var serverMsg = Encoding.UTF8.GetBytes(message);
        await socket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), WebSocketMessageType.Text, true, CancellationToken.None);
    }
}