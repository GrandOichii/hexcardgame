using System;
using System.Collections.Generic;
using Godot;
using HexClient.Match.Grid;
using HexCore.GameMatch.States;

namespace HexClient.Match.Commands;

public class SelectTileForPickTile : SelectTile {
	public SelectTileForPickTile(CommandProcessor match) : base(match)
	{
	}

	protected override bool CanSelect(Command c, Tile tile)
	{
		var args = _processor.State.Args;
		return args.Count == 0 || args.Contains(tile.CoordsStr);
	}
}
