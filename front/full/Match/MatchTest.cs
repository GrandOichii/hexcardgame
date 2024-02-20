using Godot;
using System;
using Shared;
using System.Text.Json;
using Utility;
using System.IO;

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
	public LineEdit UrlEditNode { get; private set; }

	public HttpRequest CreateMatchRequestNode { get; private set; }
	public HttpRequest GetMatchRequestNode { get; private set; }
	public HttpRequest TCPConnectRequestNode { get; private set; }
	
	#endregion
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		#region Node fetching

		MatchNode = GetNode<Match>("%Match");
		OverlayNode = GetNode<Control>("%Overlay");
		MatchIdEditNode = GetNode<LineEdit>("%MatchIdEdit");
		ErrorLabelNode = GetNode<Label>("%ErrorLabel");
		UrlEditNode = GetNode<LineEdit>("%UrlEdit");

		GetMatchRequestNode = GetNode<HttpRequest>("%GetMatchRequest");
		TCPConnectRequestNode = GetNode<HttpRequest>("%TCPConnectRequest");
		CreateMatchRequestNode = GetNode<HttpRequest>("%CreateMatchRequest");

		#endregion

		MatchNode.Visible = false;
	}
	
	private string Url => UrlEditNode.Text;

	public void Connect(string address, int port) {
		// TODO get address and port
		var client = new MatchConnection();
		client.Connect(address, port);
		var stream = client.GetStream();
		NetUtil.Read(stream);
		NetUtil.Write(stream, "tcp-player");
		NetUtil.Read(stream);
		NetUtil.Write(stream, "dev::Mana Drill#3|dev::Brute#3|dev::Mage Initiate#3|dev::Warrior Initiate#3|dev::Rogue Initiate#3|dev::Flame Eruption#3|dev::Urakshi Shaman#3|dev::Urakshi Raider#3|dev::Give Strength#3|dev::Blood for Knowledge#3|dev::Dragotha Mage#3|dev::Prophecy Scholar#3|dev::Trained Knight#3|dev::Cast Armor#3|dev::Druid Outcast#3|starters::Knowledge Tower#3|dev::Elven Idealist#3|dev::Elven Outcast#3|dev::Dub#3|dev::Barracks#3|dev::Shieldmate#3|dev::Healer Initiate#3|dev::Archdemon Priest#3|starters::Scorch the Earth#3|dev::Kobold Warrior#3|dev::Kobold Mage#3|dev::Kobold Rogue#3|starters::Dragotha Student#3|starters::Tutoring Sphinx#3|starters::Dragotha Battlemage#3|starters::Inspiration#3");
		
		MatchNode.Load(new Wrapper<MatchConnection>(client));
		OverlayNode.Visible = false;
		MatchNode.Visible = true;
	}	

	#region Signal connections

	private void OnConnectButtonPressed()
	{
		
	}
	

	private void OnGetMatchRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		var data = body.GetStringFromUtf8();
		var match = JsonSerializer.Deserialize<MatchProcess>(data, Common.JSON_SERIALIZATION_OPTIONS);
		// Connect(_address, _port);
	}

	private void OnTcpConnectRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
//		if (result != (long)HttpRequest.Result.Success) {
//			ErrorLabelNode.Text = $"Failed to fetch match (response code: {response_code})";
//			return;
//		}
	}

	private void OnCreateMatchRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		var data = body.GetStringFromUtf8();
		var match = JsonSerializer.Deserialize<MatchProcess>(data, Common.JSON_SERIALIZATION_OPTIONS);
		GD.Print(match.Id);
		var split = match.TcpAddress.Split(":");
		var address = split[0];
		var port = int.Parse(split[1]);

		Connect(address, port);
	}

	private void OnCreateMatchButtonPressed()
	{
		string[] headers = new string[] { "Content-Type: application/json" };
		var data = File.ReadAllText("create-post.json");
		CreateMatchRequestNode.Request(Url + "/match/create", headers, HttpClient.Method.Post, data);
	}
	
	#endregion
}

