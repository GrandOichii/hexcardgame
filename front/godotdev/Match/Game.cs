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

	public List<Command> Commands = new() {
		new Command("play", new() {
			new SelectCard(),
			new SelectTile()
		}),
		
		new Command("move", new() {
			new SelectTile(),
			new SelectDirection(0)
		})
	};

#nullable enable
	public Command? CurrentCommand { get; set; }
#nullable disable

	public bool Accepts(GamePart o) {
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

	public void Process(GamePart o) {
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

public interface GamePart {
}

abstract public class CommandPart {
	abstract public bool Accepts(Command c, GamePart o);
	abstract public string ToActionPart(Command c, GamePart o);
	public void Process(Command c, GamePart part) {
		c.Add(part);
	}
}

public class Command {
	public string Name { get; protected set; }
	public List<CommandPart> Parts { get; protected set; }
	public int PartI => Results.Count;
	public List<GamePart> Results { get; }=new();
	public Command(string name, List<CommandPart> parts) {
		Name = name;
		Parts = parts;
	}

	public void Reset() {
		Results.Clear();
		Game.Instance.CurrentCommand = null;
	}

	public void Add(GamePart part) {
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
	public override bool Accepts(Command c, GamePart o)
	{
		switch (o) {
			case HandCardBase hand:
				return hand.CardNode.LastState.AvailableActions.Contains("play");
			case TileBase:
				return false;
		}
		throw new Exception("Does no accept GamePart of type " + nameof(o));
	}

	public override string ToActionPart(Command c, GamePart o)
	{
		return (o as HandCardBase).CardNode.LastState.MID;
	}

}

public class SelectTile : CommandPart
{
	public override bool Accepts(Command c, GamePart o)
	{
		switch(o) {
			case HandCardBase:
				return false;
			case TileBase:
				return true;
		}
		throw new Exception("Does no accept GamePart of type " + nameof(o));
	}

	public override string ToActionPart(Command c, GamePart o)
	{
		var t = o as TileBase;
		return "" + t.Coords.Y + "." + t.Coords.X;
	}

}

public class SelectDirection : CommandPart
{
	private int _comparedToI;
	public SelectDirection(int comparedToI) {
		_comparedToI = comparedToI;
	}
	public override bool Accepts(Command c, GamePart o)
	{
		switch(o) {
			case HandCardBase:
				return false;
			case TileBase tile:
				return CanAccept(c, tile);
		}
		throw new Exception("Does no accept GamePart of type " + nameof(o));
	}

	private bool CanAccept(Command c, TileBase tile) {
		var coords = tile.Coords;
		var all_dir_arr = core.tiles.Map.DIR_ARR;
		var ii = (int)coords.Y % 2;
		var dir_arr = all_dir_arr[ii];
		var compare = (c.Results[_comparedToI] as TileBase).Coords;
		for (int i = 0; i < dir_arr.Length; i++) {
			var newC = new Vector2((int)coords.X + dir_arr[i][1], (int)coords.Y + dir_arr[i][0]);

			if (newC.IsEqualApprox(compare)) {
				return true;
			}
		}

		return false;
	}

	public override string ToActionPart(Command c, GamePart o)
	{
		var t = o as TileBase;
		var coords = t.Coords;
		var all_dir_arr = core.tiles.Map.DIR_ARR;
		var ii = (int)coords.Y % 2;
		var dir_arr = all_dir_arr[ii];
		var compare = (c.Results[_comparedToI] as TileBase).Coords;
		for (int i = 0; i < dir_arr.Length; i++) {
			var newC = new Vector2((int)coords.X + dir_arr[i][1], (int)coords.Y + dir_arr[i][0]);

			if (newC.IsEqualApprox(compare)) {
				return ((i + 3)%6).ToString();
			}
		}
		throw new Exception("Can't construct direction to " + coords.ToString() + " from " + compare);
	}

}
