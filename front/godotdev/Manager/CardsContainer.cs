using Godot;
using System;

public partial class CardsContainer : HFlowContainer
{
	static readonly PackedScene CardBaseScene = ResourceLoader.Load("res://Match/Cards/HandCardBase.tscn") as PackedScene;

	private void OnCardsRequestAddCard(CardWrapper card)
	{
		var cNode = CardBaseScene.Instantiate() as HandCardBase;
//		cNode.ProcessMode = Node.ProcessModeEnum.Disabled;
		// TODO doenst scroll when hovering over a card
		AddChild(cNode);
		MoveChild(cNode, 0);
		cNode.Load(card.Card);
	}
}


