using Godot;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace HexClient.Manager;

public partial class MatchProcessViewWindow : Window, IMatchProcessViewWindow
{
	#region Packed scenes

	[Export]
	private PackedScene ConnectedMatchWindowPS { get; set; }
	[Export]
	private PackedScene MatchViewWindowPS { get; set; }

	#endregion
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

	private async void OnMatchProcessViewConnectionCreated(Wrapper<IConnection> connectionW, string name, string deck)
	{
		var client = connectionW.Value;

		var window = ConnectedMatchWindowPS.Instantiate() as ConnectedMatchWindow;

		GetParent().AddChild(window);

		await window.Load(client, name, deck);
		window.GrabFocus();

		QueueFree();
	}

	private void OnMatchProcessViewWatcherConnectionCreated(Wrapper<HubConnection> connectionW, string matchId)
	{
		var connection = connectionW.Value;
		var window = MatchViewWindowPS.Instantiate() as MatchViewWindow;

		GetParent().AddChild(window);

		window.GrabFocus();
		_ = window.Connect(connection, matchId);
		
		QueueFree();
	}
	
	#endregion
}





