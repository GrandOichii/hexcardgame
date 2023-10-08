using core.match.states;
using Godot;
using System;

public partial class HandCard : Control
{
	#region Nodes
	
	public Card CardNode { get; private set;}
	
	#endregion
	
	public override void _Ready()
	{
		#region Node fetching

		CardNode = GetNode<Card>("%Card");
		
		#endregion

		CustomMinimumSize = CardNode.Size * CardNode.Scale;
	}

	public void Load(MCardState card) {
		CardNode.Load(card);
	}

}
