using System;
using System.Collections.Generic;
using Godot;
using HexClient.Match.Grid;
using HexCore.GameMatch.States;

namespace HexClient.Match.Commands;

public class SelectTile : CommandPart {
	public SelectTile(CommandProcessor match) : base(match)
	{
	}

	public override bool Accepts(Command c, IGamePart o)
	{
		switch(o) {
			case null:
				return false;
			case HandCard:
				return false;
			case Tile tile:
				return CanSelect(c, tile);
		}
		throw new Exception("Does no accept IGamePart of type " + nameof(o));
	}

	protected virtual bool CanSelect(Command c, Tile tile) {
		return true;
	}

	public override string ToActionPart(Command c, IGamePart o)
	{
		var t = o as Tile;
		return "" + t.Coords.Y + "." + t.Coords.X;
	}
}