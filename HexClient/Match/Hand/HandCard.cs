using Godot;
using HexCore.GameMatch.States;
using System;

namespace HexClient.Match;

public partial class HandCard : Control, IHandCard
{
	#region Nodes
	
	public Card CardNode { get; private set; }

	#endregion

	private MatchCardState _state;

	public override void _Ready()
	{
		#region Node fetching
		
		CardNode = GetNode<Card>("%Card");
		
		#endregion
		
		CustomMinimumSize = CardNode.Size * CardNode.Scale;
	}

	public void Load(MatchCardState state)
	{
		_state = state;
		CardNode.Load(state);
	}

	public void SetShowCardIds(bool v)
	{
		CardNode.SetShowMID(v);
	}

	public MatchCardState GetState()
	{
		return _state;
	}
}
