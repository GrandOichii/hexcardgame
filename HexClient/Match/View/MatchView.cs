using Godot;
using HexCore.GameMatch.States;
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
	public MatchInfoState MatchInfo { get; private set; }
	
	public override void _Ready()
	{
		#region Node fetching
		
		MatchNode = GetNode<Match>("%Match");
		
		#endregion
	}

	public async Task<bool> Connect(HubConnection connection, string matchId) {
		connection.On<string>("Config", OnViewConfig);
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

	private Task OnConnectionClosed(Exception e) {
		// TODO
		GD.Print("Connection closed");
		return Task.CompletedTask;
	}

	private Task OnViewConfig(string message) {
		MatchInfo = JsonSerializer.Deserialize<HexStates.MatchInfoState>(message, Common.JSON_SERIALIZATION_OPTIONS);
		MatchNode.LoadMatchInfo(MatchInfo);
		return Task.CompletedTask;
	}

	private Task OnViewUpdate(string message) {
		var state = JsonSerializer.Deserialize<BaseState>(message, Common.JSON_SERIALIZATION_OPTIONS);
		state.ApplyTo(MatchNode, MatchInfo);
		return Task.CompletedTask;
	}

	private Task OnViewEnd() {
		// TODO
		GD.Print("Ended view");
		return Task.CompletedTask;
	}
}
