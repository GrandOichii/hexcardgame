using System;
using System.Collections.Generic;
using Godot;
using HexClient.Match.Grid;
using HexCore.GameMatch.States;

namespace HexClient.Match.Commands;

public class SelectPlayTile : SelectTile {
	private readonly int _cardI;
	public SelectPlayTile(CommandProcessor processor, int cardI) : base(processor) {
		_cardI = cardI;
	}
	protected override bool CanSelect(Command c, ITile tile) {
		var myID = _processor.Config.MyID;
		var card = (c.Results[_cardI] as IHandCard).GetState();
		if (tile.GetState() is null) {
			return false;
		}
		if (card.Type == "Spell") {
			var caster = tile.GetState()?.Entity;
			if (caster is null) {
				return false;
			}
			if (caster?.OwnerID != myID) {
				return false;
			}
			var type = caster?.Type;
			if (!type.Contains("Mage")) {
				return false;
			}
			return true;
		}
		// card is not spell: tile has to be empty and owned by the player or has to have an enemy entity
		if (tile.GetState()?.OwnerID != myID) {
			return false;
		}
		if (tile.GetState()?.Entity is not null) {
			return false;
		}

		return true;
	}
}