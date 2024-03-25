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

	protected override bool CanSelect(Command c, Tile tile) {
		if (tile.State is null) return false;
		if (tile.State?.Entity is null) return false;
		MatchCardState en = (MatchCardState)(tile.State?.Entity);
		return en.AvailableActions.Contains("move");;
	}
}