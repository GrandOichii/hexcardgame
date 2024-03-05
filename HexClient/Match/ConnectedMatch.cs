using Godot;
using System;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Utility;

namespace HexClient.Match;

public partial class ConnectedMatch : Control
{
	#region Nodes
	
	public Match MatchNode { get; private set; }
	public LineEdit ActionEditNode { get; private set; }
	
	#endregion
	
	public IConnection Connection { get; private set; }

	private HexStates.MatchInfoState _personalConfig;
	public HexStates.MatchInfoState MatchInfo { 
		get => _personalConfig;
		private set {
			_personalConfig = value;
			MatchNode.MatchId = _personalConfig.MatchId;
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		#region Node fetching
		
		MatchNode = GetNode<Match>("%Match");
		ActionEditNode = GetNode<LineEdit>("%ActionEdit");

		#endregion
	}

	public async Task LoadConnection(IConnection connection) {
		Connection = connection;

		// TODO load configuration
		Connection.SubscribeToUpdate(OnMatchUpdate);

	}

	private static readonly string CONFIG_PREFIX = "config-";
	private async Task OnMatchUpdate(string message) {

		if (message.StartsWith(CONFIG_PREFIX)) {
			message = message[CONFIG_PREFIX.Length..];
			MatchInfo = JsonSerializer.Deserialize<HexStates.MatchInfoState>(message, Common.JSON_SERIALIZATION_OPTIONS);
			MatchNode.LoadMatchInfo(MatchInfo);
			
			return;
		}

		var state = JsonSerializer.Deserialize<MatchState>(message, Common.JSON_SERIALIZATION_OPTIONS);
		state.ApplyTo(MatchNode, MatchInfo);

		return;
	}
	
	#region Signal connections
	
	private void OnSendActionButtonPressed()
	{
		var action = ActionEditNode.Text;
		ActionEditNode.Text = "";
		Connection.Write(action);
	}

	private void OnPassButtonPressed()
	{
		Connection.Write("pass");
	}
	
	#endregion
}


