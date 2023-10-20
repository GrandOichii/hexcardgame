using Godot;
using System;

public partial class DeckEditWindow : Window
{
	#region Nodes
	
	
	
	#endregion
	
	private DeckData _current;
	
	public override void _Ready()
	{
		#region Node fethching
		
		
		
		#endregion
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void Load(DeckData deck) {
		Show();
		
		_current = deck;
		// TODO
	}
	
	#region Signal connections
	
	private void _on_close_requested()
	{
		Hide();
	}

	private void _on_add_button_pressed()
	{
		// Replace with function body.
	}
	
	#endregion
}



