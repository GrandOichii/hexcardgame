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

	protected override bool CanSelect(Command c, ITile tile)
	{
		var args = _processor.State.Args;
		var coords = tile.GetCoords();
		return args.Count == 0 || args.Contains(coords.X + "." + coords.Y);
	}
}
