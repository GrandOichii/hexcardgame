using Godot;
using System;

namespace HexClient.Client;

public partial class DisplayCard : Control, ICardDisplay
{
	#region Nodes
	
	public Card CardNode { get; private set; }

	#endregion

	public override void _Ready()
	{
		#region Node fetching
		
		CardNode = GetNode<Card>("%Card");
		
		#endregion

		CustomMinimumSize = CardNode.CustomMinimumSize;
	}

	public void Load(HexCore.Cards.ExpansionCard card)
	{
		CardNode.Load(card);
	}

	#region Signal connections

	private void OnGuiInput(InputEvent e)
	{
		if (e.IsActionPressed("context_menu_popup")) {
			GD.Print(CardNode.NameLabelNode.Text);
		}
	}

	#endregion
}



