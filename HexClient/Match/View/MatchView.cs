using Godot;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Utility;

namespace HexClient.Match.View;

public partial class MatchView : Control
{
	#region Nodes
	
	public Match MatchNode { get; private set; }
	
	#endregion

	public HubConnection Connection { get; private set; }
	
	public override void _Ready()
	{
		#region Node fetching
		
		MatchNode = GetNode<Match>("%Match");
		
		#endregion
	}

	public async Task<bool> Connect(HubConnection connection, string matchId) {				
		connection.On<string>("Update", OnViewUpdate);
		connection.On("ViewEnd", OnViewEnd);
		connection.Closed += OnConnectionClosed;

		try {
			GD.Print("Connecting...");
			await connection.StartAsync();
			await connection.SendAsync("Connect", matchId);
			GD.Print("Connected!");
		} catch (Exception e) {
			// TODO
			GD.Print("Failed to connect");
			GD.Print(e.Message);
			return false;
		}

		Connection = connection;
		return true;
	}

	private async Task OnConnectionClosed(Exception? e) {
		// TODO
		GD.Print("Connection closed");
	}

	private async Task OnViewUpdate(string data) {
		// TODO
		GD.Print("New update:");
		GD.Print(data);
		var state = JsonSerializer.Deserialize<BaseState>(data, Common.JSON_SERIALIZATION_OPTIONS);
		GD.Print("matchplayerid: " + state.CurPlayerID);
		GD.Print("");
	}

	private async Task OnViewEnd() {
		// TODO
		GD.Print("Ended view");
	}
}
