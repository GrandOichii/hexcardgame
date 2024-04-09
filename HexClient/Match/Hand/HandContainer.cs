using Godot;
using HexCore.GameMatch.States;
using System;

namespace HexClient.Match;

public partial class HandContainer : Control
{

	#region Nodes

	public Container CardContainerNode { get; private set; }

	#endregion

	public override void _Ready()
	{
		#region Node fetching

		CardContainerNode = GetNode<Container>("%CardContainer");

		#endregion
	}

	public void Load(State.MatchState state, Match match, PackedScene handCardPS) {
		Load(state.MyData, match, handCardPS);
	}

	public void Load(MyDataState state, Match match, PackedScene handCardPS) {
		var cCount = CardContainerNode.GetChildCount();
		var nCount = state.Hand.Count;

		if (nCount > cCount) {
			// fill hand up to new count
			for (int i = 0; i < nCount - cCount; i++) {
				var child = handCardPS.Instantiate();
				CardContainerNode.AddChild(child);

				var cd = child as IHandCard;
				cd.SetCommandProcessor(match.Processor);
				cd.SetShowCardIds(match.ShowCardIds);
				match.ShowCardIdsToggled += cd.SetShowCardIds;
			}
		}
		if (nCount < cCount) {
			// trim child count
			for (int i = cCount - 1; i >= nCount; i--) {
			// for (int i = nCount + 1; i < cCount; i++) {
				var child = CardContainerNode.GetChild(i);
				child.Free();
			}
		}
		// load card data
		for (int i = 0; i < nCount; i++) {
			(CardContainerNode.GetChild(i) as IHandCard).Load(state.Hand[i]);
		}
	}
}
