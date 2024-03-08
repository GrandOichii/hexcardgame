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
	public CheckButton IsBotCheckNode { get; private set; }
	public Node WindowsNode { get; private set; }
	public CheckBox WebSocketCheckNode { get; private set; }
	public CheckBox TcpCheckNode { get; private set; }
	public LineEdit PlayerNameEditNode { get; private set; }
	public MatchTable MatchTableNode { get; private set; }
	public Control BotConfigNode { get; private set; }
	public LineEdit PlayerDeckEditNode { get; private set; }
	public LineEdit BotDeckEditNode { get; private set; }
	public LineEdit BotNameEditNode { get; private set; }
	
	public HttpRequest CreateRequestNode { get; private set; }
	public HttpRequest ConnectRequestNode { get; private set; }

	public AcceptDialog FailedToConnectPopupNode { get; private set; }
	public FileDialog ChooseDeckFileDialogNode { get; private set; }
	
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
		IsBotCheckNode = GetNode<CheckButton>("%IsBotCheck");
		WebSocketCheckNode = GetNode<CheckBox>("%WebSocketCheck");
		TcpCheckNode = GetNode<CheckBox>("%TcpCheck");
		MatchTableNode = GetNode<MatchTable>("%MatchTable");
		BotConfigNode = GetNode<Control>("%BotConfig");
		PlayerDeckEditNode = GetNode<LineEdit>("%PlayerDeckEdit");
		BotDeckEditNode = GetNode<LineEdit>("%BotDeckEdit");
		BotNameEditNode = GetNode<LineEdit>("%BotNameEdit");

		ConnectRequestNode = GetNode<HttpRequest>("%ConnectRequest");
		CreateRequestNode = GetNode<HttpRequest>("%CreateRequest");

		FailedToConnectPopupNode = GetNode<AcceptDialog>("%FailedToConnectPopup");
		ChooseDeckFileDialogNode = GetNode<FileDialog>("%ChooseDeckFileDialog");
		
		#endregion
		
		GetNode<LineEdit>("%BaseUrlEdit").Text = BaseUrl;
		OnIsBotCheckToggled(IsBotCheckNode.ButtonPressed);
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

		// TODO validate deck file
		var deck = File.ReadAllText(PlayerDeckEditNode.Text);

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
		// TODO allow to change WatchConfigId
		// TODO allow to change CanWatch
		// TODO allow to change bot type
		// TODO validate bot name
		// TODO validate bot deck file

		var p1Config = new PlayerConfig {
			BotConfig = null
		};
		var p2Config = new PlayerConfig {
			BotConfig = null
		};

		if (IsBotCheckNode.ButtonPressed) {
			var botConfig = new BotConfig {
				BotType = BotType.RANDOM,
				Name = BotNameEditNode.Text,
				StrDeck = File.ReadAllText(BotDeckEditNode.Text)
			};
			p2Config.BotConfig = botConfig;
		}
		
		var result = new MatchProcessConfig
		{
			MatchConfigId = "65d9ddfe768206fe1d2482ea",
			CanWatch = true,
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
		string[] headers = new string[] { "Content-Type: application/json" };
		var config = BuildCreateMatchProcessConfig();
		var data = JsonSerializer.Serialize(config, Common.JSON_SERIALIZATION_OPTIONS);
		// var data = IsBotCheckNode.ButtonPressed ? File.ReadAllText("bot.json") : File.ReadAllText("real.json");
		CreateRequestNode.Request(BaseUrl + "match/create", headers, HttpClient.Method.Post, data);
	}

	private void OnCreateRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
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

	private void OnIsBotCheckToggled(bool buttonPressed)
	{
		// CreateTween().TweenProperty(BotConfigNode, "modulate", new Color(1, 1, 1, buttonPressed ? 1.0f : .5f), .1f);
		// BotConfigNode.SetProcess(buttonPressed);
		BotConfigNode.Visible = buttonPressed;
	}

	private void OnChoosePlayerDeckButtonPressed()
	{
		ChooseDeckFileDialogNode.SetMeta("ForReal", true);
		ChooseDeckFileDialogNode.Show();
	}

	private void OnChooseBotDeckButtonPressed()
	{
		ChooseDeckFileDialogNode.SetMeta("ForReal", false);
		ChooseDeckFileDialogNode.Show();
	}

	private void OnChooseDeckFileDialogFileSelected(string path)
	{
		var forReal = ChooseDeckFileDialogNode.GetMeta("ForReal").AsBool();
		var target = forReal ? PlayerDeckEditNode : BotDeckEditNode;
		target.Text = path;
	}
	
	#endregion
}





