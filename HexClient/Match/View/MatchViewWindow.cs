using Godot;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace HexClient.Match.View;

public partial class MatchViewWindow : Window
{
	#region Nodes
	
	public MatchView MatchViewNode { get; private set; }
	
	#endregion
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		#region Node fetching
		
		MatchViewNode = GetNode<MatchView>("%MatchView");
		
		#endregion

		Hide();
	}

	public async Task Connect(HubConnection connection, string matchId) {
		var connected = await MatchViewNode.Connect(connection, matchId);
		
		if (!connected) {
			OnCloseRequested();
			return;
		}
		Show();
	}
	
	#region Signal connections

	private void OnCloseRequested()
	{
		// TODO close connection
		QueueFree();
	}
		
	#endregion
}

