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
	
	public ConfirmationDialog DeleteCardPopupNode { get; private set; }
	
	#endregion
		
	public override void _Ready()
	{
		#region Node fetching
		
		CardDisplayNode = GetNode<DeckCardDisplay>("%CardDisplay");
		
		DeleteCardPopupNode = GetNode<ConfirmationDialog>("%DeleteCardPopup");
		
		#endregion
	}
	
	public void Load(string cid, int amount) {
		CardDisplayNode.Load(cid, amount);
	}

	#region Signal connections
	
	private void OnRemoveButtonPressed()
	{
		if (CardDisplayNode.Amount == 0) {
			DeleteCardPopupNode.DialogText = $"Are you sure you want to remove {GetCID()} from the deck?";
			DeleteCardPopupNode.Show();
			return;
		}

		--CardDisplayNode.Amount;

		EmitSignal(SignalName.AmountChanged, CardDisplayNode.Amount);
	}

	private void OnAddButtonPressed()
	{
		++CardDisplayNode.Amount;
		EmitSignal(SignalName.AmountChanged, CardDisplayNode.Amount);
	}

	public string GetCID() => CardDisplayNode.CID;
	public int GetAmount() => CardDisplayNode.Amount;
	public bool IsCardValid() => CardDisplayNode.Valid;

	public void SubcribeToAmountChanged(Action<int> action)
	{
		AmountChanged += action.Invoke;
	}

	public void SetAmount(int amount)
	{
		CardDisplayNode.Amount = amount;

		EmitSignal(SignalName.AmountChanged, CardDisplayNode.Amount);
	}

	private void OnDeleteCardPopupConfirmed()
	{
		QueueFree();
		EmitSignal(SignalName.AmountChanged, -1);
	}

	#endregion
}


