using System;
using System.Collections.Generic;
using Godot;
using HexClient.Match.Grid;
using HexCore.GameMatch.States;

namespace HexClient.Match.Commands;

public class SelectCard : CommandPart
{
	public SelectCard(CommandProcessor match) : base(match)
	{
	}

	public override bool Accepts(Command c, IGamePart o)
	{
        return o switch
        {
            IHandCard hand => hand.GetState().AvailableActions.Contains("play"), // TODO make more complex
            ITile => false,
            _ => throw new Exception("Does no accept IGamePart of type " + nameof(o)),
        };
    }

	public override string ToActionPart(Command c, IGamePart o)
	{
		return (o as IHandCard).GetState().MID;
	}

}
