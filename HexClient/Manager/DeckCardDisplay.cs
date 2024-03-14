using Godot;
using System;

namespace HexClient.Manager;

public partial class DeckCardDisplay : Control, IDeckCardDisplay
{
	#region Nodes

	public Label CIDLabelNode { get; private set; }
	public Label AmountLabelNode { get; private set; }

	#endregion

	public override void _Ready()
	{
		#region Node fetching

		CIDLabelNode = GetNode<Label>("%CIDLabel");
		AmountLabelNode = GetNode<Label>("%AmountLabel");

		#endregion
	}

	public void Load(string cid, int amount)
	{
		// TODO

		CIDLabelNode.Text = cid;
		AmountLabelNode.Text = amount.ToString();
	}
}
