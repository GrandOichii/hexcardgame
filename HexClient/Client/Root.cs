using Godot;
using System;
using System.IO;
using System.Text.Json;
using Utility;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace HexClient.Client;

public enum MatchStatus {
	WAITING_FOR_PLAYERS,
	IN_PROGRESS,
	FINISHED,
	CRASHED
}

public class MatchProcess {
	public MatchStatus Status { get; set; }
	// public MatchRecord? Record { get; private set; } = null;
	public string TcpAddress { get; set; }
	public Guid Id { get; set; }
}

public partial class Root : Control
{
	private readonly static PackedScene MatchViewWindowPS = ResourceLoader.Load<PackedScene>("res://Match/View/MatchViewWindow.tscn");
	private readonly static PackedScene ConnectedMatchWindowPS = ResourceLoader.Load<PackedScene>("res://Match/ConnectedMatchWindow.tscn");

	#region Nodes
	
	public LineEdit ConnectMatchIdEditNode { get; private set; }
	public LineEdit WatchMatchIdEditNode { get; private set; }
	public HttpRequest CreateRequestNode { get; private set; }
	public HttpRequest ConnectRequestNode { get; private set; }
	public CheckBox IsBotCheckNode { get; private set; }
	public Node WindowsNode { get; private set; }
	public CheckBox WebSocketCheckNode { get; private set; }
	public CheckBox TcpCheckNode { get; private set; }
	
	#endregion
	
	public string BaseUrl {
		get => GetNode<GlobalSettings>("/root/GlobalSettings").BaseUrl;
		set => GetNode<GlobalSettings>("/root/GlobalSettings").BaseUrl = value;
	}
	
	public override void _Ready()
	{
		#region Node fetching
		
		WindowsNode = GetNode<Node>("%Windows");
		ConnectMatchIdEditNode = GetNode<LineEdit>("%ConnectMatchIdEdit");
		WatchMatchIdEditNode = GetNode<LineEdit>("%WatchMatchIdEdit");
		IsBotCheckNode = GetNode<CheckBox>("%IsBotCheck");
		WebSocketCheckNode = GetNode<CheckBox>("%WebSocketCheck");
		TcpCheckNode = GetNode<CheckBox>("%TcpCheck");

		ConnectRequestNode = GetNode<HttpRequest>("%ConnectRequest");
		CreateRequestNode = GetNode<HttpRequest>("%CreateRequest");
		
		#endregion
		
		OnBaseUrlEditTextChanged(GetNode<LineEdit>("%BaseUrlEdit").Text);
	}

	private async Task<WebSocketConnection> CreateWebSocketConnection(MatchProcess match) {
		var client = new ClientWebSocket();
		// FIXME freezes
		GD.Print(BaseUrl + "match/connect/" + match.Id.ToString());
		// TODO ugly
		await client.ConnectAsync(new Uri(BaseUrl.Replace("http", "ws") + "match/connect/" + match.Id.ToString()), CancellationToken.None);
		GD.Print("connected!");
		var result = new WebSocketConnection(client);
		return result;
	}

	private async Task<TcpConnection> CreateTcpConnection(MatchProcess match) {
		var client = new TcpClient();
		await client.ConnectAsync(IPEndPoint.Parse(match.TcpAddress));
		var result = new TcpConnection(client);
		return result;
	}

	private async Task ConnectTo(MatchProcess match) {
		IConnection client =
			WebSocketCheckNode.ButtonPressed
			? await CreateWebSocketConnection(match)
			: await CreateTcpConnection(match)
		;
		var window = ConnectedMatchWindowPS.Instantiate() as ConnectedMatchWindow;
		WindowsNode.AddChild(window);
		await window.Load(client);
	}

	#region Signal connection

	private void OnConnectButtonPressed()
	{
		ConnectRequestNode.Request(BaseUrl + "match/" + ConnectMatchIdEditNode.Text);
	}

	private void OnBaseUrlEditTextChanged(string newText)
	{
		BaseUrl = newText;
	}

	private void OnCreateMatchButtonPressed()
	{
		string[] headers = new string[] { "Content-Type: application/json" };
		var data = IsBotCheckNode.ButtonPressed ? File.ReadAllText("bot.json") : File.ReadAllText("real.json");
		GD.Print(BaseUrl + "match/create");
		CreateRequestNode.Request(BaseUrl + "match/create", headers, HttpClient.Method.Post, data);
	}

	private void OnCreateRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		OnConnectRequestRequestCompleted(result, response_code, headers, body);
	}

	private void OnConnectRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			// TODO show message box
			GD.Print("Error! Response code: " + response_code);
			return;
		}

		var info = JsonSerializer.Deserialize<MatchProcess>(body, Common.JSON_SERIALIZATION_OPTIONS);
		GD.Print(info.Id);

		_ = ConnectTo(info);
	}

	private void OnWatchButtonPressed()
	{
		GD.Print(BaseUrl + "match/watch");
		var connection = new HubConnectionBuilder()
			.WithUrl(BaseUrl + "match/watch")
			.Build();

		var window = MatchViewWindowPS.Instantiate() as MatchViewWindow;
		WindowsNode.AddChild(window);
		_ = window.Connect(connection, WatchMatchIdEditNode.Text);
	}

	private void OnWebSocketCheckToggled(bool buttonPressed)
	{
		TcpCheckNode.ButtonPressed = !buttonPressed;
	}

	private void OnTcpCheckToggled(bool buttonPressed)
	{
		WebSocketCheckNode.ButtonPressed = !buttonPressed;
	}
	
	#endregion
}


