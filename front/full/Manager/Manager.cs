using Godot;
using System;

public partial class Manager : Control
{
	#region Signals
	
	[Signal]
	public delegate void URLUpdatedEventHandler(string url);
	
	#endregion
	
	#region Nodes
	
	public LineEdit URLEditNode { get; private set; }
	
	#endregion
	
	public override void _Ready()
	{
		#region Node fetching
		
		URLEditNode = GetNode<LineEdit>("%URLEdit");

		#endregion 
		
		UpdateURL();
		
		// TODO remove
		GetNode<TabContainer>("%TabContainer").CurrentTab = 1;
	}
	
	private void UpdateURL() {
		EmitSignal(SignalName.URLUpdated, URLEditNode.Text);
	}
	
	#region Signal connections
	
	private void _on_url_edit_text_changed(string new_text)
	{
		UpdateURL();
	}
	
	#endregion
}


