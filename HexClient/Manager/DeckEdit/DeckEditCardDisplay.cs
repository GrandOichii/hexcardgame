using Godot;
using System;

namespace HexClient.Manager;

public partial class DeckEditCardDisplay : Control, IDeckEditCardDisplay
{
	#region Nodes
	
	public DeckCardDisplay CardDisplayNode { get; private set; }
	
	#endregion
	
	private string _cid;
	private int _amount;
	
	public override void _Ready()
	{
		#region Node fetching
		
		CardDisplayNode = GetNode<DeckCardDisplay>("%CardDisplay");
		
		#endregion
	}
	
	public void Load(string cid, int amount) {
		_cid = cid;
		_amount = amount;
		CardDisplayNode.Load(_cid, _amount);
	}

	#region Signal connections
	
	private void OnRemoveButtonPressed()
	{
		CardDisplayNode.Load(_cid, --_amount);
	}

	private void OnAddButtonPressed()
	{
		CardDisplayNode.Load(_cid, ++_amount);
	}

    public string GetCID() => _cid;
    public int GetAmount() => _amount;

    #endregion
}

