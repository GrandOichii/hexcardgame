using Godot;
using System;
using System.Threading.Tasks;

public partial class ConnectedMatchWindow : Window
{
	#region Nodes
	
	public ConnectedMatch ConnectedMatchNode { get; private set; }
	
	#endregion
	
	public override void _Ready()
	{
		#region Node fetching
		
		ConnectedMatchNode = GetNode<ConnectedMatch>("%ConnectedMatch");
		
		#endregion
	}

	public async Task Load(IConnection connection, string name, string deck) {
		await ConnectedMatchNode.LoadConnection(connection, name, deck);
	}

	#region Signal connection

	private async void OnCloseRequested()
	{
		await ConnectedMatchNode.Connection.Close();
		QueueFree();
	}
	
	#endregion
}


