using Godot;
using System;

public partial class MatchWindow : Window
{
	#region Nodes
	
	public Match MatchNode { get; private set; }
	
	#endregion
	
	public override void _Ready()
	{
		#region Node fetching
		
		MatchNode = GetNode<Match>("%Match");
		
		#endregion
	}
	
	public void Connect(string url) {
		var split = url.Split(":");
		if (split.Length != 2) {
			throw new Exception("Incorrect URL format");
		}
		var address = split[0];
		var port = int.Parse(split[1]);

		var client = new MatchConnection();
		client.Connect(address, port);
		
		MatchNode.Load(new Wrapper<MatchConnection>(client));
		Show();
	}

	#region Signal connections

	private void _on_close_requested()
	{
		// TODO
		MatchNode.Client.Close();
		Hide();
		QueueFree();
	}

	#endregion
}

