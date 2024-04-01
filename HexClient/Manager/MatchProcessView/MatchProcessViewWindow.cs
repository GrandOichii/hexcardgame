using Godot;
using Microsoft.AspNetCore.SignalR.Client;
using System;

namespace HexClient.Manager;

public partial class MatchProcessViewWindow : Window, IMatchProcessViewWindow
{
	#region Nodes

	public MatchProcessView MatchProcessViewNode { get; private set; }

	#endregion
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		#region Node fetching

		MatchProcessViewNode = GetNode<MatchProcessView>("%MatchProcessView");

		#endregion

		// TODO remove
	}

	public void Load(string matchId)
	{
		Title = $"Match {matchId[..10]}";
		
		MatchProcessViewNode.Load(matchId);
	}

	#region Signal connections

	private void OnCloseRequested()
	{
		QueueFree();
	}

	private void OnMatchProcessViewConnectionCreated(Wrapper<IConnection> connectionW)
	{
		GD.Print("connection created");
	}

	private void OnMatchProcessViewWatcherConnectionCreated(Wrapper<HubConnection> connectionW, string matchId)
	{
		// Replace with function body.
	}
	
	#endregion
}





