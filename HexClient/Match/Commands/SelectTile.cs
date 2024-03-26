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
        return o switch
        {
            null => false,
            IHandCard => false,
            ITile tile => CanSelect(c, tile),
            _ => throw new Exception("Does no accept IGamePart of type " + nameof(o)),
        };
    }

	protected virtual bool CanSelect(Command c, ITile tile) {
		return true;
	}

	public override string ToActionPart(Command c, IGamePart o)
	{
		var t = o as ITile;
		return $"{t.GetCoords().X}.{t.GetCoords().Y}";
	}
}