using Godot;
using System;

namespace HexClient.Match;

public partial class HoverCard : Control, IHoverCard
{
	#region Nodes

	public Card CardNode { get; private set; }


	#endregion

	public override void _Ready()
	{
		#region Node fetching

		CardNode = GetNode<Card>("%Card");

		#endregion

		Hide();
	}

	public void Load(HexStates.MatchCardState state)
	{
		CardNode.Load(state);
		Show();
	}
}
