using Godot;
using HexCore.Decks;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Utility;

namespace HexClient.Manager;

public interface IQueuedPlayerDisplay {
	#nullable enable
	public void Load(QueuedPlayer? player);
	#nullable disable
}

public enum MatchStatus {
	WAITING_FOR_PLAYERS,
	READY_TO_START,
	IN_PROGRESS,
	FINISHED,
	CRASHED
}

public static class MatchStatusExtensions {
	public static string ToFriendlyString(this MatchStatus status) {
		return status switch
		{
			MatchStatus.WAITING_FOR_PLAYERS => "Waiting for players",
			MatchStatus.IN_PROGRESS => "In progress",
			MatchStatus.FINISHED => "Finished",
			MatchStatus.CRASHED => "Crashed",
			MatchStatus.READY_TO_START => "Ready",
			_ => "unknown status",
		};
	}
}

public class PlayerRecord {
	public required string Name { get; set; }
	public List<string> Actions { get; set; } = new();
}

public class MatchRecord {
	public string ExceptionMessage { get; set; } = "";
	public string InnerExceptionMessage { get; set; } = "";

	#nullable enable
	public string? WinnerName { get; set; }
	
	#nullable disable
	public List<PlayerRecord> Players { get; set; } = new();
}

public class MatchProcess {
	public required MatchStatus Status { get; set; }
	public required string CreatorId { get; set; }
	public required Guid Id { get; set; }
	public required DateTime? StartTime { get; set; }
	public required DateTime? EndTime { get; set; }
	public required MatchProcessConfig Config { get; set; }
	public required QueuedPlayer[] QueuedPlayers { get; set; }
	public required MatchRecord Record { get; set; }
	public required int TcpPort { get; set; }
}

public enum QueuedPlayerStatus {
	WAITING_FOR_DATA,
	READY
}

public static class QueuedPlayerStatusExtensions {
	public static string ToFriendlyString(this QueuedPlayerStatus status) {
		return status switch
		{
			QueuedPlayerStatus.WAITING_FOR_DATA => "Waiting for data",
			QueuedPlayerStatus.READY => "Ready",

			_ => "unknown status",
		};
	}
}

public class QueuedPlayer {
	public required QueuedPlayerStatus Status { get; set; }

	#nullable enable
	public required string? Name { get; set; }
	public required string? Deck { get; set; }
	#nullable disable
	
	public required bool IsBot { get; set; }
	
}

public partial class MatchProcessView : Control
{
	#region Signals

	[Signal]
	public delegate void ConnectionCreatedEventHandler(Wrapper<IConnection> connectionW, string name, string deck);
	[Signal]
	public delegate void WatcherConnectionCreatedEventHandler(Wrapper<HubConnection> connectionW, string matchId);

	#endregion

	#region Packed scenes

	[Export]
	private PackedScene QueuedPlayerDisplayPS { get; set; }

	#endregion

	#region Nodes

	public LineEdit MatchIdNode { get; private set; }
	public Label StatusLabelNode { get; private set; }
	public Container StartTimeNode { get; private set; }
	public Label StartTimeLabelNode { get; private set; }
	public Container EndTimeNode { get; private set; }
	public Label EndTimeLabelNode { get; private set; }
	public Container QueuedPlayerInfoContainerNode { get; private set; }
	public Container ConnectionContainerNode { get; private set; }
	public LineEdit NameEditNode { get; private set; }
	public LineEdit DeckEditNode { get; private set; }
	public OptionButton DeckOptionNode { get; private set; }
	public Button ConnectButtonNode { get; private set; }
	public Button WatchButtonNode { get; private set; }
	public Button ViewRecordingButtonNode { get; private set; }
	public CheckBox WebSocketCheckNode { get; private set; }
	public CheckBox TcpCheckNode { get; private set; }

	public HttpRequest FetchMatchRequestNode { get; private set; }
	public HttpRequest FetchDecksRequestNode { get; private set; }
	public HttpRequest ConnectRequestNode { get; private set; }

	public FileDialog ChooseDeckFileDialogNode { get; private set; }
	public AcceptDialog DeckErrorPopupNode { get; private set; }

	#endregion

	private string _matchId;

	public string BaseUrl {
		get => GetNode<GlobalSettings>("/root/GlobalSettings").ApiUrl;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		#region Node fetching
		MatchIdNode = GetNode<LineEdit>("%MatchId");
		
		StatusLabelNode = GetNode<Label>("%StatusLabel");
		StartTimeNode = GetNode<Container>("%StartTime");
		StartTimeLabelNode = GetNode<Label>("%StartTimeLabel");
		EndTimeNode = GetNode<Container>("%EndTime");
		EndTimeLabelNode = GetNode<Label>("%EndTimeLabel");
		QueuedPlayerInfoContainerNode = GetNode<Container>("%QueuedPlayerInfoContainer");
		ConnectionContainerNode = GetNode<Container>("%ConnectionContainer");
		NameEditNode = GetNode<LineEdit>("%NameEdit");
		DeckEditNode = GetNode<LineEdit>("%DeckEdit");
		DeckOptionNode = GetNode<OptionButton>("%DeckOption");
		ConnectButtonNode = GetNode<Button>("%ConnectButton");
		WatchButtonNode = GetNode<Button>("%WatchButton");
		ViewRecordingButtonNode = GetNode<Button>("%ViewRecordingButton");
		WebSocketCheckNode = GetNode<CheckBox>("%WebSocketCheck");
		TcpCheckNode = GetNode<CheckBox>("%TcpCheck");

		FetchMatchRequestNode = GetNode<HttpRequest>("%FetchMatchRequest");
		FetchDecksRequestNode = GetNode<HttpRequest>("%FetchDecksRequest");
		ConnectRequestNode = GetNode<HttpRequest>("%ConnectRequest");

		ChooseDeckFileDialogNode = GetNode<FileDialog>("%ChooseDeckFileDialog");
		DeckErrorPopupNode = GetNode<AcceptDialog>("%DeckErrorPopup");

		#endregion
	}

	public void Load(string matchId) {
		_matchId = matchId;

		FetchMatchInfo();
	}

	private void FetchMatchInfo() {
		FetchMatchRequestNode.Request(BaseUrl + "match/" + Uri.EscapeDataString(_matchId));
	}

	private void LoadMatch(MatchProcess match) {
		MatchIdNode.Text = match.Id.ToString();
		StatusLabelNode.Text = match.Status.ToFriendlyString();
		
		StartTimeNode.Visible = match.StartTime is not null;
		StartTimeLabelNode.Text = match.StartTime?.ToString();

		EndTimeNode.Visible = match.EndTime is not null;
		EndTimeLabelNode.Text = match.EndTime?.ToString();

		ConnectionContainerNode.Visible = match.Status <= MatchStatus.READY_TO_START;

		while (QueuedPlayerInfoContainerNode.GetChildCount() > 0)
			QueuedPlayerInfoContainerNode.RemoveChild(QueuedPlayerInfoContainerNode.GetChild(0));

		for (int i = 0; i < match.QueuedPlayers.Length; i++) {
			if (i != 0) {
				var separator = new VSeparator();
				QueuedPlayerInfoContainerNode.AddChild(separator);
			}
			var child = QueuedPlayerDisplayPS.Instantiate();
			QueuedPlayerInfoContainerNode.AddChild(child);

			var display = child as IQueuedPlayerDisplay;
			display.Load(match.QueuedPlayers[i]);
		}
	}

	private async Task ConnectTo(MatchProcess match) {
		var name = NameEditNode.Text;

		string deck;
		try {
			deck = DeckEditNode.Text;
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
			? await CreateWebSocketConnection(match)
			: await CreateTcpConnection(match)
		;
		
		EmitSignal(SignalName.ConnectionCreated, new Wrapper<IConnection>(client), name, deck);
	}

	private async Task<WebSocketConnection> CreateWebSocketConnection(MatchProcess match) {
		var client = new ClientWebSocket();

		await client.ConnectAsync(new Uri(BaseUrl
			.Replace("http://", "ws://")
			.Replace("https://", "wss://")
		+ "match/connect/" + match.Id.ToString()), CancellationToken.None);

		var result = new WebSocketConnection(client);
		return result;
	}

	private async Task<TcpConnection> CreateTcpConnection(MatchProcess match) {
		var client = new TcpClient();
		var address = GetNode<GlobalSettings>("/root/GlobalSettings").BaseUrl + ":" + match.TcpPort;
		await client.ConnectAsync(IPEndPoint.Parse(address));
		var result = new TcpConnection(client);
		return result;
	}



	#region Signal connections
	
	private void OnSearchDeckButtonPressed()
	{
		ChooseDeckFileDialogNode.Show();
	}

	private void OnRefreshDecksButtonPressed()
	{
		var token = GetNode<GlobalSettings>("/root/GlobalSettings").JwtToken;
		string[] headers = new string[] { "Content-Type: application/json", $"Authorization: Bearer {token}" };

		FetchDecksRequestNode.Request(BaseUrl + "deck", headers);
	}
	
	private void OnDeckOptionItemSelected(long index)
	{
		// TODO
	}
	
	private void OnPasteDeckButtonPressed()
	{
		if (DeckOptionNode.Selected == -1) {
			// TODO show popup
			return;
		}
		try {
			var deck = DeckOptionNode
				.GetItemMetadata(DeckOptionNode.Selected)
				.As<Wrapper<Deck>>()
				.Value;
			var text = deck.ToDeckTemplate().ToText();
			DeckEditNode.Text = text;
		} catch (Exception e) {
			// TODO show popup
			GD.Print(e.Message);
		}
	}

	private void OnConnectButtonPressed()
	{
		ConnectRequestNode.Request(BaseUrl + "match/" + MatchIdNode.Text);
	}
	
	private void OnViewRecordingButtonPressed()
	{
		// TODO
	}

	private void OnRefreshButtonPressed()
	{
		FetchMatchInfo();
	}

	private void OnFetchMatchRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			var resp = Encoding.UTF8.GetString(body);
			// TODO show popup
			GD.Print("Failed to fetch match info!");
			GD.Print("Response code: " + response_code);
			GD.Print(resp);
			return;
		}

		var data = JsonSerializer.Deserialize<MatchProcess>(body, Common.JSON_SERIALIZATION_OPTIONS);
		LoadMatch(data);
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

		while (DeckOptionNode.ItemCount > 0)
			DeckOptionNode.RemoveItem(0);

		foreach (var deck in decks) {
			DeckOptionNode.AddItem($"{deck.Name} ({deck.Id[..3]})");
			DeckOptionNode.SetItemMetadata(DeckOptionNode.ItemCount - 1, new Wrapper<Deck>(deck));
		}
	}
	
	private void OnConnectRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			var resp = Encoding.UTF8.GetString(body);
			// FailedToConnectPopupNode.DialogText = $"Failed to connect to match! (code: {response_code})\n\n{resp}";
			// FailedToConnectPopupNode.Show();
			return;
		}

		var info = JsonSerializer.Deserialize<MatchProcess>(body, Common.JSON_SERIALIZATION_OPTIONS);

		_ = ConnectTo(info);
	}

	private void OnWebSocketCheckToggled(bool toggledOn)
	{
		TcpCheckNode.ButtonPressed = !toggledOn;
	}

	private void OnTcpCheckToggled(bool toggledOn)
	{
		WebSocketCheckNode.ButtonPressed = !toggledOn;
	}

	private void OnWatchButtonPressed()
	{
		var connection = new HubConnectionBuilder()
			.WithUrl(BaseUrl + "match/watch")
			.Build();

		EmitSignal(SignalName.WatcherConnectionCreated, new Wrapper<HubConnection>(connection), MatchIdNode.Text);
		// var window = MatchViewWindowPS.Instantiate() as MatchViewWindow;
		// WindowsNode.AddChild(window);
		// _ = window.Connect(connection, WatchMatchIdEditNode.Text);
	}
	
	private void OnChooseDeckFileDialogFileSelected(string path)
	{
		DeckEditNode.Text = File.ReadAllText(path);
	}
	
	#endregion
}



