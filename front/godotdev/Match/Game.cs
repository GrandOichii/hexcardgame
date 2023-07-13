using core.cards;
using core.match.states;
using Godot;
using Shared;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

public class Game
{
	public TcpClient Client { get; }
	static public Game Instance { get; } = new Game();

	public MatchInfoState MatchInfo { get; set; }
	public MatchState LastState { get; set; }
	public HoverCardBase HoverCard { get; set; }


	public List<string> Action { get; set; }=new();
	
	private Game() {
		Client = new();
	}

	public void AddToAction(string message) {
		Action.Add(message);
	}

	public void SendAction() {
		var message = Action.ToArray().Join(" ");
		var stream = Client.GetStream();
		NetUtil.Write(stream, message);
		GD.Print("WROTE ", message);
		Action = new();
	}

	static private readonly List<Command> Commands = new() {
		new Command("play", new() {
			new SelectCard(),
			new SelectPlayTile(0)
		}),
		
		new Command("move", new() {
			new SelectMovable(),
			new SelectDirection(0)
		})
	};

#nullable enable
	public Command? CurrentCommand { get; set; }
#nullable disable

	public bool Accepts(IGamePart o) {
		if (CurrentCommand is null) {
			foreach (var command in Commands) {
				var part = command.Parts[0];
				if (part.Accepts(command, o)) {
					return true;
				}
			}
			return false;
		}

		return CurrentCommand.Parts[CurrentCommand.PartI].Accepts(CurrentCommand, o);
	}

	public void Process(IGamePart o) {
		if (CurrentCommand is null) {
			foreach (var command in Commands) {
				var part = command.Parts[0];
				if (part.Accepts(command, o)) {
					CurrentCommand = command;
					part.Process(command, o);
					return;
				}
			}
			// TODO? throw exception
			return;
		}

		CurrentCommand.Parts[CurrentCommand.PartI].Process(CurrentCommand, o);
	}
}

public interface IGamePart {}

abstract public class CommandPart {
	abstract public bool Accepts(Command c, IGamePart o);
	abstract public string ToActionPart(Command c, IGamePart o);
	public void Process(Command c, IGamePart part) {
		c.Add(part);
	}
}

public class Command {
	public string Name { get; protected set; }
	public List<CommandPart> Parts { get; protected set; }
	public int PartI => Results.Count;
	public List<IGamePart> Results { get; }=new();
	public Command(string name, List<CommandPart> parts) {
		Name = name;
		Parts = parts;
	}

	public void Reset() {
		Results.Clear();
		Game.Instance.CurrentCommand = null;
	}

	public void Add(IGamePart part) {
		Results.Add(part);
		if (PartI == Parts.Count) {
			var words = new string[PartI];
			for (int i = 0; i < Results.Count; i++) {
				words[i] = Parts[i].ToActionPart(this, Results[i]);
			}
			var stream = Game.Instance.Client.GetStream();
			var message = Name + " " + words.Join(" ");
			NetUtil.Write(stream, message);
			GD.Print("Wrote ", message);
			Reset();
		}
	}
}

public class SelectCard : CommandPart
{
	public override bool Accepts(Command c, IGamePart o)
	{
		switch (o) {
			case HandCardBase hand:
				// TODO make more complex
				return hand.CardNode.LastState.AvailableActions.Contains("play");
			case TileBase:
				return false;
		}
		throw new Exception("Does no accept IGamePart of type " + nameof(o));
	}

	public override string ToActionPart(Command c, IGamePart o)
	{
		return (o as HandCardBase).CardNode.LastState.MID;
	}

}

public class SelectTile : CommandPart {
	public override bool Accepts(Command c, IGamePart o)
	{
		switch(o) {
			case null:
				return false;
			case HandCardBase:
				return false;
			case TileBase tile:
				return CanSelect(c, tile);
		}
		throw new Exception("Does no accept IGamePart of type " + nameof(o));
	}

	protected virtual bool CanSelect(Command c, TileBase tile) {
		return true;
	}

	public override string ToActionPart(Command c, IGamePart o)
	{
		var t = o as TileBase;
		return "" + t.Coords.Y + "." + t.Coords.X;
	}
}

public class SelectPlayTile : SelectTile {
	private readonly int _cardI;
	public SelectPlayTile(int cardI) {
		_cardI = cardI;
	}
	protected override bool CanSelect(Command c, TileBase tile) {
		var myID = Game.Instance.MatchInfo.MyID;
		var card = (c.Results[_cardI] as HandCardBase).CardNode.LastState;
		if (tile.LastState is null) {
			return false;
		}
		if (card.Type == "Spell") {
			var caster = tile.LastState?.Entity;
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
		if (tile.LastState?.OwnerID != myID) {
			return false;
		}
		if (tile.LastState?.Entity is not null) {
			return false;
		}

		return true;
	}
}

public class SelectMovable : SelectTile {
	protected override bool CanSelect(Command c, TileBase tile) {
		if (tile.LastState is null) return false;
		if (tile.LastState?.Entity is null) return false;
		MCardState en = (MCardState)(tile.LastState?.Entity);
		return en.AvailableActions.Contains("move");;
	}
}

public class SelectDirection : CommandPart
{
	private readonly int _comparedToI;
	public SelectDirection(int comparedToI) {
		_comparedToI = comparedToI;
	}
	public override bool Accepts(Command c, IGamePart o)
	{
		switch(o) {
			case HandCardBase:
				return false;
			case TileBase tile:
				return CanAccept(c, tile);
		}
		throw new Exception("Does no accept IGamePart of type " + nameof(o));
	}

	static public int GetDirection(TileBase from, TileBase to) {
		var coords = to.Coords;
		var all_dir_arr = core.tiles.Map.DIR_ARR;
		var ii = (int)coords.Y % 2;
		var dir_arr = all_dir_arr[ii];
		var compare = from.Coords;
		for (int i = 0; i < dir_arr.Length; i++) {
			var newC = new Vector2((int)coords.X + dir_arr[i][1], (int)coords.Y + dir_arr[i][0]);

			if (newC.IsEqualApprox(compare)) {
				return (i + 3) % 6;
			}
		}

		return -1;
	}

	private bool CanAccept(Command c, TileBase tile) {
		if (tile.LastState is null) return false;
		if (tile.LastState?.Entity is not null) {
			if (tile.LastState?.Entity?.OwnerID == Game.Instance.MatchInfo.MyID)
			return false;
		}
		return GetDirection(c.Results[_comparedToI] as TileBase, tile) != -1;
	}

	public override string ToActionPart(Command c, IGamePart o)
	{
		var t = o as TileBase;
		var d = GetDirection(c.Results[_comparedToI] as TileBase, t);
		if (d == -1) throw new Exception("Can't construct direction to " + t.Coords.ToString() + " from " + (c.Results[_comparedToI] as TileBase).Coords);
		return d.ToString();
	}
}


