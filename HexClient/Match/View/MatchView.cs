using Godot;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Utility;

namespace HexClient.Match.View;

public partial class MatchView : Control
{
	#region Signals
	
	[Signal]
	public delegate void ClosedEventHandler();
	
	#endregion
	
	#region Nodes
	
	public Match MatchNode { get; private set; }
	
	public AcceptDialog ForbiddenPopupNode { get; private set; }
	public AcceptDialog EndPopupNode { get; private set; }
	public AcceptDialog ConnectionFailedPopupNode { get; private set; }
	public AcceptDialog DisconnectedPopupNode { get; private set; }
	
	#endregion

	public HubConnection Connection { get; private set; }
	public HexStates.MatchInfoState MatchInfo { get; private set; }
	
	public override void _Ready()
	{
		#region Node fetching
		
		MatchNode = GetNode<Match>("%Match");

		ForbiddenPopupNode = GetNode<AcceptDialog>("%ForbiddenPopup");
		EndPopupNode = GetNode<AcceptDialog>("%EndPopup");
		ConnectionFailedPopupNode = GetNode<AcceptDialog>("%ConnectionFailedPopup");
		DisconnectedPopupNode = GetNode<AcceptDialog>("%DisconnectedPopup");
		
		#endregion
	}

	public async Task<bool> Connect(HubConnection connection, string matchId) {
		connection.On<string>("Config", OnViewConfig);
		connection.On<string>("Update", OnViewUpdate);
		connection.On<string>("ConnectFail", OnViewConnectFail);
		connection.On("Forbidden", OnViewForbidden);
		connection.On<MatchStatus, string>("EndView", OnEndView);
		connection.Closed += OnConnectionClosed;

		try {
			await connection.StartAsync();
			await connection.SendAsync("Connect", matchId);
		} catch (Exception e) {
			ConnectionFailedPopupNode.DialogText = $"Failed to connect to match!\n\n{e.Message}";
			ConnectionFailedPopupNode.Show();
			return false;
		}

		Connection = connection;
		return true;
	}

	private Task OnConnectionClosed(Exception e) {
		DisconnectedPopupNode.CallDeferred("set_text", $"Connection closed!\n\n{e.Message}");
		DisconnectedPopupNode.CallDeferred("show");

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

	private Task OnViewConnectFail(string message) {
		ConnectionFailedPopupNode.CallDeferred("set_text", $"Failed to connect to match!\n\n{message}");
		ConnectionFailedPopupNode.CallDeferred("show");

		return Task.CompletedTask;
	}

	private Task OnViewForbidden() {
		ForbiddenPopupNode.CallDeferred("show");
		return Task.CompletedTask;
	}

	private Task OnEndView(MatchStatus status, string winnerName) {
		EndPopupNode.CallDeferred("set_text", status == MatchStatus.CRASHED ? "Match crashed." : $"Match ended!\nWinner: {winnerName}");
		EndPopupNode.CallDeferred("show");
		
		return Task.CompletedTask;
	}

	public async Task CloseConnection() {
		await Connection.DisposeAsync();
	}
	
	#region Signal connections

	private void OnForbiddenPopupConfirmed()
	{
		CallDeferred("emit_signal", SignalName.Closed);
	}
	
	private void OnForbiddenPopupCanceled()
	{
		CallDeferred("emit_signal", SignalName.Closed);
	}

	private void OnEndPopupCanceled()
	{
		CallDeferred("emit_signal", SignalName.Closed);
	}

	private void OnEndPopupConfirmed()
	{
		CallDeferred("emit_signal", SignalName.Closed);
	}

	private void OnConnectionFailedPopupCanceled()
	{
		CallDeferred("emit_signal", SignalName.Closed);
	}

	private void OnConnectionFailedPopupConfirmed()
	{
		CallDeferred("emit_signal", SignalName.Closed);
	}

	private void OnDisconnectedPopupConfirmed()
	{
		CallDeferred("emit_signal", SignalName.Closed);
	}
	
	#endregion
}

