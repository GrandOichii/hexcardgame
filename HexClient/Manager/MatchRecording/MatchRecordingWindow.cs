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
		Load("12ddcbb9-255b-4303-9b2e-7db1351a82ee");
	}

	public void Load(string matchId) {
		MatchRecordingNode.Load(matchId);
	}
}
