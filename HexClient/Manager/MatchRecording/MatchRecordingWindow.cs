using Godot;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace HexClient.Manager;

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
		Load("7bfbedf6-db4c-4bdc-be47-5ad254ed5048");
	}

	public void Load(string matchId) {
		MatchRecordingNode.Load(matchId);
	}
}
