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
using System.Collections.Generic;

namespace HexClient.Manager.Tabs;

public interface IMatchRecordDisplayWindow {
	public void Load(MatchRecord record);
}

public partial class MatchesTab : Control
{
	#region Packed scenes

	[Export]
	private PackedScene MatchViewWindowPS { get; set; }
	[Export]
	private PackedScene ConnectedMatchWindowPS { get; set; }
	[Export]
	private PackedScene MatchRecordDisplayWindowPS { get; set; }

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
	public CheckBox AutoConnectCheckNode { get; private set; }
	public SpinBox BatchEditNode { get; private set; }
	public Button RemoveCrashedButtonNode { get; private set; }
	public OptionButton SavedDecksOptionNode { get; private set; }
	
	public HttpRequest CreateRequestNode { get; private set; }
	public HttpRequest ConnectRequestNode { get; private set; }
	public HttpRequest FetchBasicConfigRequestNode { get; private set; }
	public HttpRequest RemoveCrashedRequestNode { get; private set; }
	public HttpRequest FetchDecksRequestNode { get; private set; }

	public AcceptDialog FailedToConnectPopupNode { get; private set; }
	public AcceptDialog FailedToCreatePopupNode { get; private set; }
	public AcceptDialog DeckErrorPopupNode { get; private set; }
	public AcceptDialog FailedToFetchBasicConfigPopupNode { get; private set; }
	public FileDialog ChooseDeckFileDialogNode { get; private set; }

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
		AutoConnectCheckNode = GetNode<CheckBox>("%AutoConnectCheck");
		BatchEditNode = GetNode<SpinBox>("%BatchEdit");
		RemoveCrashedButtonNode = GetNode<Button>("%RemoveCrashedButton");
		SavedDecksOptionNode = GetNode<OptionButton>("%SavedDecksOption");

		ConnectRequestNode = GetNode<HttpRequest>("%ConnectRequest");
		CreateRequestNode = GetNode<HttpRequest>("%CreateRequest");
		FetchBasicConfigRequestNode = GetNode<HttpRequest>("%FetchBasicConfigRequest");
		RemoveCrashedRequestNode = GetNode<HttpRequest>("%RemoveCrashedRequest");
		FetchDecksRequestNode = GetNode<HttpRequest>("%FetchDecksRequest");

		FailedToConnectPopupNode = GetNode<AcceptDialog>("%FailedToConnectPopup");
		FailedToCreatePopupNode = GetNode<AcceptDialog>("%FailedToCreatePopup");
		DeckErrorPopupNode = GetNode<AcceptDialog>("%DeckErrorPopup");
		FailedToFetchBasicConfigPopupNode = GetNode<AcceptDialog>("%FailedToFetchBasicConfigPopup");
		ChooseDeckFileDialogNode = GetNode<FileDialog>("%ChooseDeckFileDialog");

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

		await client.ConnectAsync(new Uri(BaseUrl
			.Replace("http://", "ws://")
			.Replace("https://", "wss://")
		+ "match/connect/" + match.Id.ToString()), CancellationToken.None);

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
			deck = PlayerDeckEditNode.Text;
			_ = DeckTemplate.FromText(deck);
		} catch (DeckParseException e) {
			DeckErrorPopupNode.DialogText = $"Failed to load deck file!\n\n{e.Message}";
			DeckErrorPopupNode.Show();
			
			return;
		} catch (FileNotFoundException e) {
			DeckErrorPopupNode.DialogText = $"Failed to load deck file!\n\n{e.Message}";
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
		PlayerConfig1Node.ActionDelaySpinNode.Value = 0;

		var p2Config = PlayerConfig2Node.Baked;
		PlayerConfig2Node.ActionDelaySpinNode.Value = 0;
		
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

	private async void OnCreateMatchButtonPressed()
	{
		for (int i = 0; i < BatchEditNode.Value; i++) {
			MatchProcessConfig config;
			try {
				config = BuildCreateMatchProcessConfig();
			} catch (DeckParseException e) {
				DeckErrorPopupNode.DialogText = $"Failed to load deck file!\n\n{e.Message}";
				DeckErrorPopupNode.Show();

				return;
			} catch (FileNotFoundException e) {
				DeckErrorPopupNode.DialogText = $"Failed to load deck file!\n\n{e.Message}";
				DeckErrorPopupNode.Show();

				return;
			}

			string[] headers = new string[] { "Content-Type: application/json" };
			var data = JsonSerializer.Serialize(config, Common.JSON_SERIALIZATION_OPTIONS);
			CreateRequestNode.Request(BaseUrl + "match/create", headers, HttpClient.Method.Post, data);
			await ToSignal(CreateRequestNode, "request_completed");
		}
	}

	private void OnCreateRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			var resp = Encoding.UTF8.GetString(body);
			FailedToCreatePopupNode.DialogText = $"Failed to create match! (code: {response_code})\n\n{resp}";
			FailedToCreatePopupNode.Show();

			return;
		}

		var info = JsonSerializer.Deserialize<MatchProcess>(body, Common.JSON_SERIALIZATION_OPTIONS);
		ConnectMatchIdEditNode.Text = info.Id.ToString();
		WatchMatchIdEditNode.Text = info.Id.ToString();

		if (
			PlayerConfig1Node.IsBotCheckNode.ButtonPressed && 
			PlayerConfig2Node.IsBotCheckNode.ButtonPressed
		) return;
			
		if (!AutoConnectCheckNode.ButtonPressed) return;

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
		if (response_code != 200) {
			var resp = Encoding.UTF8.GetString(body);
			FailedToFetchBasicConfigPopupNode.DialogText = $"Failed to fetch basic match config! (code: {response_code})\n\n{resp}";
			FailedToFetchBasicConfigPopupNode.Show();

			return;
		}
		
		var configId = Encoding.UTF8.GetString(body);
		MatchConfigIdEditNode.Text = configId;
	}

	private void OnFetchBasicConfigButtonPressed()
	{
		FetchBasicConfigRequestNode.Request(BaseUrl + "config/basic");
	}

	private void OnChoosePlayerDeckButtonPressed()
	{
		ChooseDeckFileDialogNode.Show();
	}

	private void OnChooseDeckFileDialogFileSelected(string path)
	{
		PlayerDeckEditNode.Text = File.ReadAllText(path);
	}

	private void OnRemoveCrashedButtonPressed()
	{
		var token = GetNode<GlobalSettings>("/root/GlobalSettings").JwtToken;
		string[] headers = new string[] { "Content-Type: application/json", $"Authorization: Bearer {token}" };

		RemoveCrashedRequestNode.Request(BaseUrl + "match/crashed", headers, HttpClient.Method.Delete);
	}

	private void OnRemoveCrashedRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code == 200) return;

		// TODO show popup
		GD.Print(response_code);
	}

	private void OnFetchDecksRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			// TODO show popup
			var resp = Encoding.UTF8.GetString(body);
			GD.Print(response_code);
			GD.Print(resp);
			return;
		}

		var decks = JsonSerializer.Deserialize<List<Deck>>(body, Common.JSON_SERIALIZATION_OPTIONS);

		while (SavedDecksOptionNode.ItemCount > 0)
			SavedDecksOptionNode.RemoveItem(0);

		foreach (var deck in decks) {
			SavedDecksOptionNode.AddItem($"{deck.Name} ({deck.Id[..3]})");
			SavedDecksOptionNode.SetItemMetadata(SavedDecksOptionNode.ItemCount - 1, new Wrapper<Deck>(deck));
		}

	}

	private void OnRefreshDecksButtonPressed()
	{
		var token = GetNode<GlobalSettings>("/root/GlobalSettings").JwtToken;
		string[] headers = new string[] { "Content-Type: application/json", $"Authorization: Bearer {token}" };

		FetchDecksRequestNode.Request(BaseUrl + "deck", headers);
	}

	private void OnPasteFromDeckButtonPressed()
	{
		if (SavedDecksOptionNode.Selected == -1) {
			// TODO show popup
			return;
		}
		try {
			var deck = SavedDecksOptionNode
				.GetItemMetadata(SavedDecksOptionNode.Selected)
				.As<Wrapper<Deck>>()
				.Value;
			var text = deck.ToDeckTemplate().ToText();
			PlayerDeckEditNode.Text = text;
		} catch (Exception e) {
			// TODO show popup
			GD.Print(e.Message);
		}
	}

	private void OnMatchTableMatchActivated(Wrapper<MatchProcess> match)
	{
		// TODO? this allows to view the same record from 2 different windows, change?

		var window = MatchRecordDisplayWindowPS.Instantiate() as Window;
		WindowsNode.AddChild(window);
		
		window.Title = $"Match record {match.Value.Id.ToString()[..3]}";
		
		var display = window as IMatchRecordDisplayWindow;
		display.Load(match.Value.Record);
	}

	#endregion
}

