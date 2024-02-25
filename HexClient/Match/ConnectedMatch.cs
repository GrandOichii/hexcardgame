using Godot;
using System;

namespace HexClient.Match;

public partial class ConnectedMatch : Control
{
	#region Nodes
	
	public Match MatchNode { get; private set; }
	
	#endregion
	
	private IConnection _connection;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		#region Node fetching
		
		MatchNode = GetNode<Match>("%Match");
		
		#endregion
	}

	public void LoadConnection(IConnection connection) {
		_connection = connection;

		// TODO load configuration
	}
}
