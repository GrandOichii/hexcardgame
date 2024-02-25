using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace HexClient;

public interface IConnection {

    public delegate void MessageHandler(string message);
    public delegate void CloseHandler();
    public Task Write(string message);
    public void SubscribeToUpdate(MessageHandler func);
    public void SubscriveToClose(CloseHandler func);
}

public class WebSocketConnection : IConnection
{
    private readonly ClientWebSocket _client;

    #nullable enable
    private event IConnection.MessageHandler? OnReceive;
    #nullable disable

    private event IConnection.CloseHandler? OnClose;

    public WebSocketConnection(ClientWebSocket client)
    {
        _client = client;

        // TODO don't know if this is the best way of doing this
        _ = Task.Run(async () =>
        {
            while (_client.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;
                var buffer = new ArraySegment<byte>(new byte[1024]);
                var message = new StringBuilder();
                do
                {
                    result = await _client.ReceiveAsync(buffer, CancellationToken.None);
                    string messagePart = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                    message.Append(message);
                }
                while (!result.EndOfMessage);
                OnReceive?.Invoke(message.ToString());
            }
            OnClose?.Invoke();
        });

    }

    public void SubscribeToUpdate(IConnection.MessageHandler func)
    {
        OnReceive += func;
    }

    public async Task Write(string message)
    {
        await _client.SendAsync(message.ToUtf8Buffer(), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public void SubscriveToClose(IConnection.CloseHandler func)
    {
        OnClose += func;
    }
}

// public class TcpConnection : IConnection
// {
//     private readonly TcpClient _client;

//     public TcpConnection(TcpClient client) {
//         _client = client;
//     }

//     public Task Write(string message)
//     {
//         // TODO
//         var client = new ClientWebSocket();
//         client.ConnectAsync
//         throw new System.NotImplementedException();
//     }
// }