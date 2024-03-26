using System;
using System.Collections.Generic;
using Godot;
using HexClient.Match.Grid;
using HexCore.GameMatch.States;

namespace HexClient.Match.Commands;


public class SelectDirection : CommandPart
{
	private readonly int _comparedToI;
	public SelectDirection(CommandProcessor match, int comparedToI) : base(match) {
		_comparedToI = comparedToI;
	}
	public override bool Accepts(Command c, IGamePart o)
	{
        return o switch
        {
            IHandCard => false,
            ITile tile => CanAccept(c, tile),
            _ => throw new Exception("Does no accept IGamePart of type " + nameof(o)),
        };
    }

	static public int GetDirection(ITile from, ITile to) {
		var coords = to.GetCoords();
		var all_dir_arr = HexCore.GameMatch.Tiles.Map.DIR_ARR;
		var ii = (int)coords.X % 2;
		var dir_arr = all_dir_arr[ii];
		var compare = from.GetCoords();
		for (int i = 0; i < dir_arr.Length; i++) {
			var newC = new Vector2((int)coords.X + dir_arr[i][0], (int)coords.Y + dir_arr[i][1]);

			if (newC.IsEqualApprox(compare)) {
				return (i + 3) % 6;
			}
		}

		return -1;
	}

	private bool CanAccept(Command c, ITile tile) {
		if (tile.GetState() is null) return false;
		if (tile.GetState()?.Entity is not null) {
			if (tile.GetState()?.Entity?.OwnerID == _processor.Config.MyID)
			return false;
		}
		return GetDirection(c.Results[_comparedToI] as ITile, tile) != -1;
	}

	public override string ToActionPart(Command c, IGamePart o)
	{
		var t = o as ITile;
		var d = GetDirection(c.Results[_comparedToI] as ITile, t);
		if (d == -1) throw new Exception("Can't construct direction to " + t.GetCoords().ToString() + " from " + (c.Results[_comparedToI] as ITile).GetCoords());
		return d.ToString();
	}
}
