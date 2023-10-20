using Godot;
using System;

public partial class DeckCard : Control
{
	// #region Signals
	
	// [Signal]
	// public delegate void AmountChangedEventHandler(string cid, int v);
	
	// #endregion
	
	#region Nodes
	
	public Label NameLabelNode { get; private set; }
	public SpinBox CountBoxNode { get; private set; }
	
	#endregion

	public DeckCardData Data { get; private set; }
	
	private string _cID;

	public int Amount {
		get => (int)CountBoxNode.Value;
		set => CountBoxNode.Value = value;
	}
	
	public override void _Ready()
	{
		#region Node fetching
		
		NameLabelNode = GetNode<Label>("%NameLabel");
		CountBoxNode = GetNode<SpinBox>("%CountBox");
		
		#endregion
	}
	
	public void Load(DeckCardData card) {
		Data = card;
		_cID = card.Card.Expansion + "::" + card.Card.Card.Name;
		
		NameLabelNode.Text = _cID;
		CountBoxNode.Value = card.Amount;
	}

	public DeckCardData Baked {
		get {
			var result = new DeckCardData();

			result.Amount = Amount;
			result.Card = Data.Card;

			return result;
		}
	}
	
	#region Signal connections
	
	private void _on_count_box_value_changed(double v)
	{
		// EmitSignal(SignalName.AmountChanged, _cID, (int)v);
		if (v < 0)
			QueueFree();
	}

	#endregion
}


