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
		
		#endregion
	}

	public async Task<bool> Connect(HubConnection connection, string matchId) {
		connection.On<string>("Config", OnViewConfig);
		connection.On<string>("Update", OnViewUpdate);
		connection.On<string>("ConnectFail", OnViewConnectFail);
		connection.On("Forbidden", OnViewForbidden);
		connection.On("EndView", OnEndView);
		connection.Closed += OnConnectionClosed;

		try {
			await connection.StartAsync();
			await connection.SendAsync("Connect", matchId);
		} catch (Exception e) {
			ConnectionFailedPopupNode.Show();
			GD.Print("Failed to connect!");
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

	private Task OnViewConnectFail(string message) {
		ConnectionFailedPopupNode.CallDeferred("set_text", $"Failed to connect to match!\n\n{message}");
		ConnectionFailedPopupNode.CallDeferred("show");

		return Task.CompletedTask;
	}

	private Task OnViewForbidden() {
		ForbiddenPopupNode.CallDeferred("show");
		return Task.CompletedTask;
	}

	private Task OnEndView() {
		// TODO set winner text/match status
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
	
	#endregion
}

