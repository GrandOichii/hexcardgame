using Godot;
using System;
using System.Net.Sockets;

public partial class MatchHub : Control
{
	#region Nodes
	
	public ItemList MatchListNode { get; private set; }
	public Window ConnectToMatchWindowNode { get; private set; }
	public LineEdit MatchURLEditNode { get; private set; }
	public Control MatchListContainerNode { get; private set; }
	
	#endregion

	#region Signals

	[Signal]
	public delegate void NewMatchConnectionEventHandler(Wrapper<MatchConnection> connection);

	#endregion
	
	public override void _Ready()
	{
		#region Node fetching
		
		MatchListNode = GetNode<ItemList>("%MatchList");
		ConnectToMatchWindowNode = GetNode<Window>("%ConnectToMatchWindow");
		MatchURLEditNode = GetNode<LineEdit>("%MatchURLEdit");
		MatchListContainerNode = GetNode<Control>("%MatchListContainer");
		
		#endregion
	}
	
	#region Signal connections
	
	private void _on_connect_button_pressed()
	{
		try {
		
			var url = MatchURLEditNode.Text;
			var split = url.Split(":");
			if (split.Length != 2) {
				throw new Exception("Incorrect URL format");
			}
			var address = split[0];
			var port = int.Parse(split[1]);

			var client = new MatchConnection();
			client.Connect(address, port);

			ConnectToMatchWindowNode.Hide();
			EmitSignal(SignalName.NewMatchConnection, new Wrapper<MatchConnection>(client));
		
		} catch (Exception e) {
			GUtil.Alert(this, "Failed to connect: " + e, "Failed to connect to match");
		}
 	}

	private void _on_connect_to_match_window_close_requested()
	{
		ConnectToMatchWindowNode.Hide();
	}

	private void _on_new_connection_button_pressed()
	{
		ConnectToMatchWindowNode.Show();
	}

	private void _on_toggle_match_list_button_pressed()
	{
		MatchListContainerNode.Visible = !MatchListContainerNode.Visible;
	}
	
	#endregion
}




