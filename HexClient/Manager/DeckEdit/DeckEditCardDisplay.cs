using Godot;
using System;

namespace HexClient.Manager;

public partial class DeckEditCardDisplay : Control, IDeckEditCardDisplay
{
	#region Signals

	[Signal]
	public delegate void AmountChangedEventHandler(int amount);

	#endregion

	#region Nodes
	
	public DeckCardDisplay CardDisplayNode { get; private set; }
	
	#endregion
	
	private int _amount;
	
	public override void _Ready()
	{
		#region Node fetching
		
		CardDisplayNode = GetNode<DeckCardDisplay>("%CardDisplay");
		
		#endregion
	}
	
	public void Load(string cid, int amount) {
		_amount = amount;
		CardDisplayNode.Load(cid, _amount);
	}

	#region Signal connections
	
	private void OnRemoveButtonPressed()
	{
		if (_amount == 0) return;

		CardDisplayNode.Load(CardDisplayNode.CID, --_amount);
		EmitSignal(SignalName.AmountChanged, _amount);
	}

	private void OnAddButtonPressed()
	{
		CardDisplayNode.Load(CardDisplayNode.CID, ++_amount);
		EmitSignal(SignalName.AmountChanged, _amount);
	}

	public string GetCID() => CardDisplayNode.CID;
	public int GetAmount() => _amount;

	public bool IsCardValid() => CardDisplayNode.Valid;

	public void SubcribeToAmountChanged(Action<int> action)
	{
		AmountChanged += action.Invoke;
	}

    public void SetAmount(int amount)
    {
		_amount = amount;
		CardDisplayNode.AmountLabelNode.Text = _amount.ToString();
    }

    #endregion
}

