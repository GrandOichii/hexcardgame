using System.Net.WebSockets;
using System.Text;

namespace ManagerBack.Extensions;

public static class WebSocketExtensions {
    public static async Task<string> Read(this WebSocket socket, CancellationToken cancellationToken) {
        if (socket.State == WebSocketState.Closed)  {
            // TODO
            throw new Exception("socket us closed");
        }
        var buffer = new byte[1024 * 4];
        var response = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
        if (response.MessageType == WebSocketMessageType.Close) {
            // TODO check
            // socket.close
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "SocketClose", CancellationToken.None);
            return "";
        }
        return Encoding.UTF8.GetString(buffer).Replace("\0", string.Empty);
    }

    public static async Task<string> Read(this WebSocket socket) {
        return await Read(socket, CancellationToken.None);
    }


    public static async Task Write(this WebSocket socket, string message) {
        var serverMsg = Encoding.UTF8.GetBytes(message);
        await socket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), WebSocketMessageType.Text, true, CancellationToken.None);
    }
    public static async Task Write(this WebSocket socket, string message, CancellationToken cancellationToken) {
        var serverMsg = Encoding.UTF8.GetBytes(message);
        await socket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), WebSocketMessageType.Text, true, cancellationToken);
    }
}