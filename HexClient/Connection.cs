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
	public void SubscriveToClose(CloseHandler func);
	public Task Close();
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

	public void SubscriveToClose(IConnection.CloseHandler func)
	{
		OnClose += func;
	}

	public async Task Close()
	{
		await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
	}
}

public class TcpConnection : IConnection
{
	private readonly TcpClient _client;
	private event IConnection.MessageHandler? OnReceive;

	public TcpConnection(TcpClient client) {
		_client = client;
		_client.ReceiveTimeout = 20;

		_ = Task.Run(() => {
			while (_client.Connected) {
				try {
					var message = NetUtil.Read(_client.GetStream());
					GD.Print("read " + message);
					OnReceive?.Invoke(message);
				} catch {
					continue;
				}
			}
		});
	}

	public void SubscribeToUpdate(IConnection.MessageHandler func)
	{
		// TODO
		// throw new NotImplementedException();
		OnReceive += func;
	}

	public void SubscriveToClose(IConnection.CloseHandler func)
	{
		// TODO
		// throw new NotImplementedException();
	}

	public Task Write(string message)
	{
		NetUtil.Write(_client.GetStream(), message);
		return Task.CompletedTask;
	}

	public Task Close()
	{
		_client.Close();
		return Task.CompletedTask;
	}
}
