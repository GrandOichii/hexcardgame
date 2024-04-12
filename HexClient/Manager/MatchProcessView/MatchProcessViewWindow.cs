using Godot;
using HexClient.Manager.Recording;
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
	[Export]
	private PackedScene MatchRecordingWindowPS { get; set; }

	#endregion
	#region Nodes

	public MatchProcessView MatchProcessViewNode { get; private set; }

	#endregion

	private string _matchId;

	public override void _Ready()
	{
		#region Node fetching

		MatchProcessViewNode = GetNode<MatchProcessView>("%MatchProcessView");

		#endregion
	}

	public void Load(string matchId)
	{
		_matchId = matchId;
		Title = $"Match {matchId[..3]}";
		
		MatchProcessViewNode.Load(matchId);
	}

	#region Signal connections

	private void OnCloseRequested()
	{
		QueueFree();
	}

	private async void OnMatchProcessViewConnectionCreated(Wrapper<IConnection> connectionW, string name, string deck, string password)
	{
		var client = connectionW.Value;

		var window = ConnectedMatchWindowPS.Instantiate() as ConnectedMatchWindow;

		GetParent().AddChild(window);

		await window.Load(client, name, deck, password);
		window.GrabFocus();

		// !FIXME if two views are open and viewing same match process and one view connects to match, the whole app crashes
		// !FIXME this is the problem, find a way to fix
		// GetParent().RemoveChild(this);
		// QueueFree();
	}

	private void OnMatchProcessViewWatcherConnectionCreated(Wrapper<HubConnection> connectionW, string matchId)
	{
		var connection = connectionW.Value;
		var window = MatchViewWindowPS.Instantiate() as MatchViewWindow;

		GetParent().AddChild(window);

		window.GrabFocus();
		_ = window.Connect(connection, matchId);
		
		// QueueFree();
	}
	
	private void OnMatchProcessViewRecordingRequested(string matchId)
	{
		var window = MatchRecordingWindowPS.Instantiate() as MatchRecordingWindow;
		
		GetParent().AddChild(window);
		window.Load(matchId);

		// !FIXME crashes, same as OnMatchProcessViewConnectionCreated
		// QueueFree();
	}

	public string GetMatchId()
	{
		return _matchId;
	}

	public void Focus()
	{
		GrabFocus();
	}

	#endregion
}
