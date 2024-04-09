using Godot;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace HexClient.Manager.Recording;

public partial class MatchRecordingWindow : Window
{
	#region Nodes
	
	public MatchRecording MatchRecordingNode { get; private set; }
	
	#endregion
	
	public override void _Ready()
	{
		#region Node fetching
		
		MatchRecordingNode = GetNode<MatchRecording>("%MatchRecording");
		
		#endregion
		
		GrabFocus();
	}

	public void Load(string matchId) {
		MatchRecordingNode.Load(matchId);
	}
	
	#region Signal connections
	
	private void OnCloseRequested()
	{
		QueueFree();
	}
	
	#endregion
}

