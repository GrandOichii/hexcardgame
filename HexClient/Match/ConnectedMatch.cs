using Godot;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

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

	private async Task Connect() {
		var client = new ClientWebSocket();
		// TODO format base url
		await client.ConnectAsync(new Uri("http://localhost:5239/api/v1/match/connect/1"), CancellationToken.None);
	}

	public void LoadConnection(IConnection connection) {
		_connection = connection;

		// TODO load configuration
	}
}
