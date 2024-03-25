using Godot;
using System;

namespace HexClient;

public partial class MatchRecordDisplayWindow : Window, IMatchRecordDisplayWindow
{
	#region Nodes
	
	public MatchRecordDisplay MatchRecordDisplayNode { get; private set; }
	
	#endregion
	
	public override void _Ready()
	{
		#region Node fetching
		
		MatchRecordDisplayNode = GetNode<MatchRecordDisplay>("%MatchRecordDisplay");
		
		#endregion
	}

	public void Load(MatchRecord record) {
		MatchRecordDisplayNode.Load(record);
	}
	
	#region Signal connections
	
	private void OnCloseRequested()
	{
		QueueFree();
	}
	
	#endregion
}


