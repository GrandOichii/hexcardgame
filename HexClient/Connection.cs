using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Shared;

namespace HexClient.Connection;

public interface IConnection {

	public delegate Task MessageHandler(string message);
	public delegate void CloseHandler();
	public Task Write(string message);
	public void SubscribeToUpdate(MessageHandler func);
	public void SubscribeToClose(CloseHandler func);
	public Task Close();
}

public class WebSocketConnection : IConnection
{
	private readonly ClientWebSocket _client;

	#nullable enable
	private event IConnection.MessageHandler? OnReceive;

	private event IConnection.CloseHandler? OnClose;
	#nullable disable

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
					message.Append(messagePart);
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

	public void SubscribeToClose(IConnection.CloseHandler func)
	{
		OnClose += func;
	}

	public async Task Close()
	{
		await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "ClientClose", CancellationToken.None);
	}
}

public class TcpConnection : IConnection
{
	private readonly TcpClient _client;

	#nullable enable
	private event IConnection.MessageHandler? OnReceive;
	private event IConnection.CloseHandler? OnClose;
	#nullable disable

	public TcpConnection(TcpClient client) {
		_client = client;
		_client.ReceiveTimeout = 20;

		_ = Task.Run(() => {
			while (_client.Connected) {
				try {
					var message = NetUtil.Read(_client.GetStream());
					OnReceive?.Invoke(message);
				} catch {
					continue;
				}
			}
			OnClose?.Invoke();
		});
	}

	public void SubscribeToUpdate(IConnection.MessageHandler func)
	{
		OnReceive += func;
	}

	public void SubscribeToClose(IConnection.CloseHandler func)
	{
		OnClose += func;
	}

	public Task Write(string message)
	{
		NetUtil.Write(_client.GetStream(), message);
		GD.Print("writing " + message);
		return Task.CompletedTask;
	}

	public Task Close()
	{
		_client.Close();
		return Task.CompletedTask;
	}
}
