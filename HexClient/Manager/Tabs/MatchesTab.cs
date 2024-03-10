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
using System.Text;
using HexCore.Decks;

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
	public Node WindowsNode { get; private set; }
	public CheckBox WebSocketCheckNode { get; private set; }
	public CheckBox TcpCheckNode { get; private set; }
	public LineEdit PlayerNameEditNode { get; private set; }
	public MatchTable MatchTableNode { get; private set; }
	public LineEdit PlayerDeckEditNode { get; private set; }
	public LineEdit MatchConfigIdEditNode { get; private set; }
	public CheckBox CanWatchCheckNode { get; private set; }
	
	public HttpRequest CreateRequestNode { get; private set; }
	public HttpRequest ConnectRequestNode { get; private set; }
	public HttpRequest FetchBasicConfigRequestNode { get; private set; }

	public AcceptDialog FailedToConnectPopupNode { get; private set; }
	public AcceptDialog FailedToCreatePopupNode { get; private set; }
	public AcceptDialog DeckErrorPopupNode { get; private set; }

	public PlayerConfig PlayerConfig1Node { get; private set; }
	public PlayerConfig PlayerConfig2Node { get; private set; }
	
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
		WebSocketCheckNode = GetNode<CheckBox>("%WebSocketCheck");
		TcpCheckNode = GetNode<CheckBox>("%TcpCheck");
		MatchTableNode = GetNode<MatchTable>("%MatchTable");
		PlayerDeckEditNode = GetNode<LineEdit>("%PlayerDeckEdit");
		MatchConfigIdEditNode = GetNode<LineEdit>("%MatchConfigIdEdit");
		CanWatchCheckNode = GetNode<CheckBox>("%CanWatchCheck");

		ConnectRequestNode = GetNode<HttpRequest>("%ConnectRequest");
		CreateRequestNode = GetNode<HttpRequest>("%CreateRequest");
		FetchBasicConfigRequestNode = GetNode<HttpRequest>("%FetchBasicConfigRequest");

		FailedToConnectPopupNode = GetNode<AcceptDialog>("%FailedToConnectPopup");
		FailedToCreatePopupNode = GetNode<AcceptDialog>("%FailedToCreatePopup");
		DeckErrorPopupNode = GetNode<AcceptDialog>("%DeckErrorPopup");

		PlayerConfig1Node = GetNode<PlayerConfig>("%PlayerConfig1");
		PlayerConfig2Node = GetNode<PlayerConfig>("%PlayerConfig2");
		
		#endregion
		
		GetNode<LineEdit>("%BaseUrlEdit").Text = BaseUrl;

		PlayerConfig1Node.BotNameEditNode.Text += "1";
		PlayerConfig2Node.BotNameEditNode.Text += "2";
		
		OnLiveMatchesButtonPressed();
		OnFetchBasicConfigButtonPressed();
	}

	private async Task<WebSocketConnection> CreateWebSocketConnection(MatchProcess match, string name, string deck) {
		var client = new ClientWebSocket();
		// TODO ugly
		await client.ConnectAsync(new Uri(BaseUrl.Replace("http://", "ws://") + "match/connect/" + match.Id.ToString()), CancellationToken.None);
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

		string deck;
		try {
			deck = File.ReadAllText(PlayerDeckEditNode.Text);
			_ = DeckTemplate.FromText(deck);
		} catch (DeckParseException e) {
			// TODO more detailed message
			DeckErrorPopupNode.DialogText = $"Failed to connect to match!\n\n{e.Message}";
			DeckErrorPopupNode.Show();
			
			return;
		} catch (FileNotFoundException e) {
			// TODO more detailed message
			DeckErrorPopupNode.DialogText = $"Failed to connect to match!\n\n{e.Message}";
			DeckErrorPopupNode.Show();

			return;
		}


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

	private MatchProcessConfig BuildCreateMatchProcessConfig() {

		var p1Config = PlayerConfig1Node.Baked;
		var p2Config = PlayerConfig2Node.Baked;
		
		var result = new MatchProcessConfig
		{
			MatchConfigId = MatchConfigIdEditNode.Text,
			CanWatch = CanWatchCheckNode.ButtonPressed,
			P1Config = p1Config,
			P2Config = p2Config,
		};

		return result;
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
		
		MatchProcessConfig config;
		try {
			config = BuildCreateMatchProcessConfig();
		} catch (DeckParseException e) {
			// TODO more detailed message
			DeckErrorPopupNode.DialogText = $"Failed to create match!\n\n{e.Message}";
			DeckErrorPopupNode.Show();

			return;
		} catch (FileNotFoundException e) {
			// TODO more detailed message
			DeckErrorPopupNode.DialogText = $"Failed to create match!\n\n{e.Message}";
			DeckErrorPopupNode.Show();

			return;
		}

		string[] headers = new string[] { "Content-Type: application/json" };
		var data = JsonSerializer.Serialize(config, Common.JSON_SERIALIZATION_OPTIONS);
		CreateRequestNode.Request(BaseUrl + "match/create", headers, HttpClient.Method.Post, data);
	}

	private void OnCreateRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		// TODO check response code
		if (response_code != 200) {
			var resp = Encoding.UTF8.GetString(body);
			FailedToCreatePopupNode.DialogText = $"Failed to create match! (code: {response_code})\n\n{resp}";
			FailedToCreatePopupNode.Show();

			return;
		}


		// * ugly
		if (PlayerConfig1Node.IsBotCheckNode.ButtonPressed && PlayerConfig2Node.IsBotCheckNode.ButtonPressed)
			return;

		OnConnectRequestRequestCompleted(result, response_code, headers, body);
	}

	private void OnConnectRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			var resp = Encoding.UTF8.GetString(body);
			FailedToConnectPopupNode.DialogText = $"Failed to connect to match! (code: {response_code})\n\n{resp}";
			FailedToConnectPopupNode.Show();
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

	private void OnFetchBasicConfigRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		// TODO check response code
		
		var configId = Encoding.UTF8.GetString(body);
		MatchConfigIdEditNode.Text = configId;
	}

	private void OnFetchBasicConfigButtonPressed()
	{
		FetchBasicConfigRequestNode.Request(BaseUrl + "config/basic");
	}

	#endregion
}








