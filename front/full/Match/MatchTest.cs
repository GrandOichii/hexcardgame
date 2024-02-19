using Godot;
using System;
using Shared;
using System.Text.Json;
using Utility;

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
	public Guid Id { get; }
}

public partial class MatchTest : Node
{
	#region Nodes

	public Match MatchNode { get; private set; }
	public Control OverlayNode { get; private set; }
	public Label ErrorLabelNode { get; private set; }
	public LineEdit MatchIdEditNode { get; private set; }

	// public HttpRequest CreateMatchRequestNode { get; private set; }
	public HttpRequest GetMatchRequestNode { get; private set; }
	
	#endregion
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		#region Node fetching

		MatchNode = GetNode<Match>("%Match");
		OverlayNode = GetNode<Control>("%Overlay");
		MatchIdEditNode = GetNode<LineEdit>("%MatchIdEdit");
		ErrorLabelNode = GetNode<Label>("%ErrorLabel");

		GetMatchRequestNode = GetNode<HttpRequest>("%GetMatchRequest");

		#endregion

		MatchNode.Visible = false;
	}

	public void Connect(string address, int port) {
		// TODO get address and port
		var client = new MatchConnection();
		client.Connect(address, port);
		var stream = client.GetStream();
		NetUtil.Read(stream);
		NetUtil.Write(stream, "dev::Mana Drill#3|dev::Brute#3|dev::Mage Initiate#3|dev::Warrior Initiate#3|dev::Rogue Initiate#3|dev::Flame Eruption#3|dev::Urakshi Shaman#3|dev::Urakshi Raider#3|dev::Give Strength#3|dev::Blood for Knowledge#3|dev::Dragotha Mage#3|dev::Prophecy Scholar#3|dev::Trained Knight#3|dev::Cast Armor#3|dev::Druid Outcast#3|starters::Knowledge Tower#3|dev::Elven Idealist#3|dev::Elven Outcast#3|dev::Dub#3|dev::Barracks#3|dev::Shieldmate#3|dev::Healer Initiate#3|dev::Archdemon Priest#3|starters::Scorch the Earth#3|dev::Kobold Warrior#3|dev::Kobold Mage#3|dev::Kobold Rogue#3|starters::Dragotha Student#3|starters::Tutoring Sphinx#3|starters::Dragotha Battlemage#3|starters::Inspiration#3");
		NetUtil.Read(stream);
		NetUtil.Write(stream, "tcp-player");

		OverlayNode.Visible = false;
		MatchNode.Visible = true;
	}	

	#region Signal connections

	private void OnConnectButtonPressed()
	{
		// TODO request the match from the server
		// if success, connect to it using tcp
		// if error, log it to the error label
		
		GetMatchRequestNode.Request($"http://localhost:5239/api/v1/match/{MatchIdEditNode.Text}");
	}

	private void OnGetMatchRequestNodeRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		if (result != (long)HttpRequest.Result.Success) {
			ErrorLabelNode.Text = $"Failed to fetch match (response code: {response_code})";
			return;
		}
		var data = body.GetStringFromUtf8();
		var match = JsonSerializer.Deserialize<MatchProcess>(data, Common.JSON_SERIALIZATION_OPTIONS);
		var split = match.TcpAddress.Split(":");
		var address = split[0];
		var port = int.Parse(split[1]);
		Connect(address, port);
	}

	#endregion
}


