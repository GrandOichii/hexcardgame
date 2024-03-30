using Godot;
using System;
using System.Text;
using System.Text.Json;
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

	public HttpRequest FetchMatchRequestNode { get; private set; }

	#endregion

	private string _matchId;

	public string BaseUrl {
		get => GetNode<GlobalSettings>("/root/GlobalSettings").BaseUrl;
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

		FetchMatchRequestNode = GetNode<HttpRequest>("%FetchMatchRequest");

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

	#region Signal connections
	
	private void OnSearchDeckButtonPressed()
	{
		// TODO
	}

	private void OnRefreshDecksButtonPressed()
	{
		// TODO
	}
	
	private void OnDeckOptionItemSelected(long index)
	{
		// TODO
	}
	
	private void OnPasteDeckButtonPressed()
	{
		// TODO
	}

	private void OnConnectButtonPressed()
	{
		// TODO
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
	
	#endregion
}
