using System;
using System.Collections.Generic;
using Godot;
using HexClient.Match.Grid;
using HexCore.GameMatch.States;

namespace HexClient.Match.Commands;

public class SelectMovable : SelectTile {
	public SelectMovable(CommandProcessor match) : base(match)
	{
	}

	protected override bool CanSelect(Command c, ITile tile) {
		if (tile.GetState() is null) return false;
		if (tile.GetState()?.Entity is null) return false;
		MatchCardState en = (MatchCardState)(tile.GetState()?.Entity);
		return en.AvailableActions.Contains("move");;
	}
}