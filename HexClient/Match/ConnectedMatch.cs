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

	private HexCore.GameMatch.States.MatchInfoState _personalConfig;
	public HexCore.GameMatch.States.MatchInfoState PersonalConfig { 
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

	public async Task LoadConnection(IConnection connection, string name, string deck) {
		Connection = connection;

		// TODO load configuration
		Connection.SubscribeToUpdate(OnMatchUpdate);

		await Connection.Write(name);

		await Connection.Write(deck);
	}

	private static string CONFIG_PREFIX = "config-";
	private Task OnMatchUpdate(string message) {
		if (message == "deck") return Task.CompletedTask;
		if (message == "name") return Task.CompletedTask;
		if (message.StartsWith(CONFIG_PREFIX)) {
			message = message[CONFIG_PREFIX.Length..];
			PersonalConfig = JsonSerializer.Deserialize<HexCore.GameMatch.States.MatchInfoState>(message, Common.JSON_SERIALIZATION_OPTIONS);
			
			GD.Print("match id: " + PersonalConfig.MatchId);
			return Task.CompletedTask;
		}

		var state = JsonSerializer.Deserialize<MatchState>(message, Common.JSON_SERIALIZATION_OPTIONS);
		state.ApplyTo(MatchNode);

		return Task.CompletedTask;
	}
	
	#region Signal connections
	
	private void OnSendActionButtonPressed()
	{
		var action = ActionEditNode.Text;
		ActionEditNode.Text = "";
		Connection.Write(action);
	}
	
	#endregion
}

