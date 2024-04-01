using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using HexClient.Manager;
using Shared;
using Utility;

namespace HexClient.Connection;

class PlayerData {
    public required string Name { get; set; }
    public required string Deck { get; set; }
}

public interface IConnection {

	public delegate Task MessageHandler(string message);
	public void StartReceiveLoop();
	public delegate void CloseHandler();
	public Task Write(string message);
	public void SubscribeToUpdate(MessageHandler func);
	public void SubscribeToClose(CloseHandler func);
	public Task Close();
	public Task<string> Read();

	public async Task SendData(string name, string deck) {
		var pData = new PlayerData {
			Name = name,
			Deck = deck,
		};
		var data = JsonSerializer.Serialize(pData, Common.JSON_SERIALIZATION_OPTIONS);
		await Write(data);
	}
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
	}

	public void StartReceiveLoop() {
		Task.Run(async () =>
		{
			while (_client.State == WebSocketState.Open)
			{
				var message = await Read();
				GD.Print("read " + message);
				GD.Print(OnReceive);
				await OnReceive?.Invoke(message);
			}
			OnClose?.Invoke();
		});
	}

	public async Task<string> Read() {
		WebSocketReceiveResult result;
		var buffer = new ArraySegment<byte>(new byte[1024]);
		var message = new StringBuilder();
		do
		{
			result = await _client.ReceiveAsync(buffer, CancellationToken.None);
			string messagePart = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
			// messagePart = messagePart.Replace("\0", string.Empty);
			GD.Print(result.EndOfMessage);
			message.Append(messagePart);
		}
		while (!result.EndOfMessage);
		return message.ToString();
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

	public Task Close()
	{
		try {
			// TODO? not awaiting the call fixes the window not closing until the socket is closed
			_ = _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "ClientClose", CancellationToken.None);
		} catch {}
		
		return Task.CompletedTask;
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
	}

	public void StartReceiveLoop() {
		Task.Run(async () => {
			// _client.ReceiveTimeout = 50;
			while (_client.Connected) {
				try {
					var message = await Read();
					await OnReceive?.Invoke(message);
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
		return Task.CompletedTask;
	}

	public Task Close()
	{
		_client.Close();
		return Task.CompletedTask;
	}

	public Task<string> Read() {
		var result = NetUtil.Read(_client.GetStream());
		return Task.FromResult(result);
	}
}
