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
		
		Load("5c91b0ff-c4e4-4edc-9f45-bf6b5e259c37");
	}

	public void Load(string matchId) {
		MatchRecordingNode.Load(matchId);
	}
}
