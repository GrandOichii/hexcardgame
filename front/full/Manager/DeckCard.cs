using Godot;
using System;

public partial class DeckCard : Control
{
	#region Signals
	
	[Signal]
	public delegate void AmountChangedEventHandler(string cid, int v);
	
	#endregion
	
	#region Nodes
	
	public Label NameLabelNode { get; private set; }
	public SpinBox CountBoxNode { get; private set; }
	
	#endregion
	
	private string _cID;
	
	public override void _Ready()
	{
		#region Node fetching
		
		NameLabelNode = GetNode<Label>("%NameLabel");
		CountBoxNode = GetNode<SpinBox>("%CountBox");
		
		#endregion
	}
	
	public void Load(string cID, int amount) {
		_cID = cID;
		
		NameLabelNode.Text = cID;
		CountBoxNode.Value = amount;
	}
	
	#region Signal connections
	
	private void _on_count_box_value_changed(double v)
	{
		EmitSignal(SignalName.AmountChanged, _cID, (int)v);
		if (v < 0)
			QueueFree();
	}

	#endregion
}


