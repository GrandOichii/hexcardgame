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
using HexClient.Tables;

namespace HexClient.Manager.Tabs;

public partial class MatchesTab : Control
{
	#region Packed scenes

	[Export]
	private PackedScene MatchViewWindowPS { get; set; }
	[Export]
	private PackedScene ConnectedMatchWindowPS { get; set; }

	#endregion

	#region Nodes
	
	public LineEdit ConnectMatchIdEditNode { get; private set; }
	public LineEdit WatchMatchIdEditNode { get; private set; }
	public HttpRequest CreateRequestNode { get; private set; }
	public HttpRequest ConnectRequestNode { get; private set; }
	public CheckBox IsBotCheckNode { get; private set; }
	public Node WindowsNode { get; private set; }
	public CheckBox WebSocketCheckNode { get; private set; }
	public CheckBox TcpCheckNode { get; private set; }
	public LineEdit PlayerNameEditNode { get; private set; }
	public MatchTable MatchTableNode { get; private set; }
	
	#endregion
	
	public string BaseUrl {
		get => GetNode<GlobalSettings>("/root/GlobalSettings").BaseUrl;
		set => GetNode<GlobalSettings>("/root/GlobalSettings").BaseUrl = value;
	}
	
	public override void _Ready()
	{
		#region Node fetching
		
		PlayerNameEditNode = GetNode<LineEdit>("%PlayerNameEdit");
		WindowsNode = GetNode<Node>("%Windows");
		ConnectMatchIdEditNode = GetNode<LineEdit>("%ConnectMatchIdEdit");
		WatchMatchIdEditNode = GetNode<LineEdit>("%WatchMatchIdEdit");
		IsBotCheckNode = GetNode<CheckBox>("%IsBotCheck");
		WebSocketCheckNode = GetNode<CheckBox>("%WebSocketCheck");
		TcpCheckNode = GetNode<CheckBox>("%TcpCheck");
		MatchTableNode = GetNode<MatchTable>("%MatchTable");

		ConnectRequestNode = GetNode<HttpRequest>("%ConnectRequest");
		CreateRequestNode = GetNode<HttpRequest>("%CreateRequest");
		
		#endregion
		
		OnBaseUrlEditTextChanged(BaseUrl);
	}

	private async Task<WebSocketConnection> CreateWebSocketConnection(MatchProcess match, string name, string deck) {
		var client = new ClientWebSocket();
		// FIXME freezes
		// TODO ugly
		await client.ConnectAsync(new Uri(BaseUrl.Replace("http", "ws") + "match/connect/" + match.Id.ToString()), CancellationToken.None);
		GD.Print("connected!");
		var result = new WebSocketConnection(client, name, deck);
		return result;
	}

	private async Task<TcpConnection> CreateTcpConnection(MatchProcess match, string name, string deck) {
		var client = new TcpClient();
		await client.ConnectAsync(IPEndPoint.Parse(match.TcpAddress));
		var result = new TcpConnection(client, name, deck);
		return result;
	}

	private async Task ConnectTo(MatchProcess match) {
		var name = PlayerNameEditNode.Text;

		// TODO choose deck
		var deck = "dev::Mana Drill#3|dev::Brute#3|dev::Mage Initiate#3|dev::Warrior Initiate#3|dev::Rogue Initiate#3|dev::Flame Eruption#3|dev::Urakshi Shaman#3|dev::Urakshi Raider#3|dev::Give Strength#3|dev::Blood for Knowledge#3|dev::Dragotha Mage#3|dev::Prophecy Scholar#3|dev::Trained Knight#3|dev::Cast Armor#3|dev::Druid Outcast#3|starters::Knowledge Tower#3|dev::Elven Idealist#3|dev::Elven Outcast#3|dev::Dub#3|dev::Barracks#3|dev::Shieldmate#3|dev::Healer Initiate#3|dev::Archdemon Priest#3|starters::Scorch the Earth#3|dev::Kobold Warrior#3|dev::Kobold Mage#3|dev::Kobold Rogue#3|starters::Dragotha Student#3|starters::Tutoring Sphinx#3|starters::Dragotha Battlemage#3|starters::Inspiration#3";

		IConnection client =
			WebSocketCheckNode.ButtonPressed
			? await CreateWebSocketConnection(match, name, deck)
			: await CreateTcpConnection(match, name, deck)
		;
		var window = ConnectedMatchWindowPS.Instantiate() as ConnectedMatchWindow;
		WindowsNode.AddChild(window);

		await window.Load(client);
		window.GrabFocus();
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
		ConnectMatchIdEditNode.Text = info.Id.ToString();
		WatchMatchIdEditNode.Text = info.Id.ToString();

		_ = ConnectTo(info);
	}

	private void OnWatchButtonPressed()
	{
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

	private void OnLiveMatchesButtonPressed()
	{
		_ = MatchTableNode.Connect(BaseUrl + "match/live");
	}
	
	#endregion
}



