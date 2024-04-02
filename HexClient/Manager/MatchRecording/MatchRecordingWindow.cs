using Godot;
using System;

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
		
		Load("d9eb316e-4b7c-4526-b442-4efc87f8bde5");
	}
	
	public void Load(string matchId) {
		MatchRecordingNode.Load(matchId);
	}

	
}
