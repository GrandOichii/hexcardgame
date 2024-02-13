using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.Serialization.Conventions;

namespace ManagerBack.Controllers;

public class Global {
    private static readonly int MAX_CONNECTIONS = 2;
    private Global() {}
    public static Global Instance { get; } = new();

    public List<WebSocket> Connections { get; set; } = new();
    private int _cur = 0;
    
    private async Task KeepAlive(WebSocket socket) {
        while (socket.State == WebSocketState.Open) {
            System.Console.WriteLine("Requested response");
            var buf = new ArraySegment<byte>(new byte[1024]);
            var ret = await socket.ReceiveAsync(buf, CancellationToken.None);

            if (ret.MessageType == WebSocketMessageType.Close)
            {
                break;
            }                        
            System.Console.WriteLine("Received response");
        }
        Connections.Remove(socket);
        socket.Dispose();
    } 

    public async Task ConnectUser(WebSocket socket) {
        Connections.Add(socket);
        await Write(socket, "handshake");
        KeepAlive(socket);
        var name = await Read(socket);
        System.Console.WriteLine(name + " connected!");
        System.Console.WriteLine(Connections.Count + " " + MAX_CONNECTIONS);
        if (CanConnect) return;
        System.Console.WriteLine("Started run");

        Run();
    }

    public bool CanConnect { get => Connections.Count < MAX_CONNECTIONS; }

    public async Task Run() {
        while (true) {
            // Request message to the current user
            var response = await Read(Connections[_cur]);

            for (int i = 0; i < MAX_CONNECTIONS; ++i) {
                if (i == _cur) continue;

                await Write(Connections[i], response);
            }
        }
    }

    static private async Task<string> Read(WebSocket socket) {
        var buffer = new byte[1024 * 4];
        var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        // if (result.CloseStatus.HasValue) throw new Exception("connection closed");

        return Encoding.UTF8.GetString(buffer);
    }

    static private async Task Write(WebSocket socket, string message) {
        var serverMsg = Encoding.UTF8.GetBytes(message);

        await socket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), WebSocketMessageType.Text, true, CancellationToken.None);
    }
}

[ApiController]
[Route("/api/v1/wstest")]
public class WebsocketTestController : ControllerBase {
    [HttpGet("connect")]
    public async Task Connect() {
        if (HttpContext.WebSockets.IsWebSocketRequest) {
            if (Global.Instance.CanConnect) {
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                Console.WriteLine("Established ws connection!");
                // await Echo(webSocket);
                await Global.Instance.ConnectUser(webSocket);
            }
        } else {
            HttpContext.Response.StatusCode = 400;
        }
    }

    private async Task Echo(WebSocket socket) {
        var buffer = new byte[1024 * 4];
        var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        Console.WriteLine("Message received from Client");

        if (!result.CloseStatus.HasValue)
        {
            var serverMsg = Encoding.UTF8.GetBytes($"Server: Hello. You said: {Encoding.UTF8.GetString(buffer)}");
            await socket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
            Console.WriteLine("Message sent to Client");

            buffer = new byte[1024 * 4];
            result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            Console.WriteLine("Message received from Client");
            
        }
        // await socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        // Console.WriteLine("WebSocket connection closed");
 
    }

}