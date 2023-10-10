using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;
using core.match.states;
using Godot;
using Shared;

public partial class Wrapper<T> : Node {
	public T Value { get; set; }
	public Wrapper(T v) { Value = v; }
}

public static class GUtil {
	public static void Alert(Node parent, string message, string title)
	{
		OS.Alert(message, title);
	}
}

public class MatchConnection : TcpClient {
	public static readonly Color P1Color = Colors.Green;
	public static readonly Color P2Color = Colors.Red;
	public Dictionary<string, Color> PlayerColors { get; set; } = new() {
		{ "", Colors.White },
		{ "1", P1Color },
		{ "2", P2Color },
	};
	private Dictionary<string, List<Command>> _requestCommandMap;
	private MatchState _state;
	public MatchState State { 
		get => _state; 
		set {
			_state = value;
			_commands = _requestCommandMap[_state.Request];
		}
	}
	public MatchInfoState Config { get; set; }
	public Command? CurrentCommand { get; set; }
	private List<Command> _commands = new();

	public HoverCard HoverCard { get; set; }
	public MatchConnection() : base() {
		_requestCommandMap = new() {
			{
				"action", new() {
					new Command(this, "play", new() {
						new SelectCard(this),
						new SelectPlayTile(this, 0)
					}),
					
					new Command(this, "move", new() {
						new SelectMovable(this),
						new SelectDirection(this, 0)
					})
				}},
			{
				"pt", new() {
					new Command(this, "", new() {
						new SelectTileForPickTile(this)
					})
				}
			},
			{
				"update", new() {}
			}
		};
	}

	public bool Accepts(IGamePart o) {
		if (CurrentCommand is null) {
			foreach (var command in _commands) {
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
			foreach (var command in _commands) {
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

	// private MatchState _lastState;
	// public MatchState State {
	// 	get => _lastState;
	// 	set {
	// 		_lastState = value;
	// 	}
	// }
	
}

public interface IGamePart {}

abstract public class CommandPart {
	abstract public bool Accepts(Command c, IGamePart o);
	abstract public string ToActionPart(Command c, IGamePart o);
	public void Process(Command c, IGamePart part) {
		c.Add(part);
	}
	protected MatchConnection _match;
	public CommandPart(MatchConnection match) {
		_match = match;
	}
}

public class Command {
	public string Name { get; protected set; }
	public List<CommandPart> Parts { get; protected set; }
	public int PartI => Results.Count;
	public List<IGamePart> Results { get; } = new();
	private MatchConnection _connection;
	public Command(MatchConnection connection, string name, List<CommandPart> parts) {
		_connection = connection;
		Name = name;
		Parts = parts;
	}

	public void Reset() {
		Results.Clear();
		_connection.CurrentCommand = null;
	}

	public void Add(IGamePart part) {
		Results.Add(part);
		if (PartI == Parts.Count) {
			var words = new string[PartI];
			for (int i = 0; i < Results.Count; i++) {
				words[i] = Parts[i].ToActionPart(this, Results[i]);
			}
			var stream = _connection.GetStream();
			var message = Name + " " + words.Join(" ");
			NetUtil.Write(stream, message);
			GD.Print("Wrote ", message);
			Reset();
		}
	}
}

public class SelectCard : CommandPart
{
	public SelectCard(MatchConnection match) : base(match)
	{
	}

	public override bool Accepts(Command c, IGamePart o)
	{
		switch (o) {
			case HandCard hand:
				// TODO make more complex
				return hand.CardNode.State.AvailableActions.Contains("play");
			case Tile:
				return false;
		}
		throw new Exception("Does no accept IGamePart of type " + nameof(o));
	}

	public override string ToActionPart(Command c, IGamePart o)
	{
		return (o as HandCard).CardNode.State.MID;
	}

}

public class SelectTile : CommandPart {
	public SelectTile(MatchConnection match) : base(match)
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

public class SelectPlayTile : SelectTile {
	private readonly int _cardI;
	public SelectPlayTile(MatchConnection match, int cardI) : base(match) {
		_cardI = cardI;
	}
	protected override bool CanSelect(Command c, Tile tile) {
		var myID = _match.Config.MyID;
		var card = (c.Results[_cardI] as HandCard).CardNode.State;
		if (tile.State is null) {
			return false;
		}
		if (card.Type == "Spell") {
			var caster = tile.State?.Entity;
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
		if (tile.State?.OwnerID != myID) {
			return false;
		}
		if (tile.State?.Entity is not null) {
			return false;
		}

		return true;
	}
}

public class SelectMovable : SelectTile {
	public SelectMovable(MatchConnection match) : base(match)
	{
	}

	protected override bool CanSelect(Command c, Tile tile) {
		if (tile.State is null) return false;
		if (tile.State?.Entity is null) return false;
		MCardState en = (MCardState)(tile.State?.Entity);
		return en.AvailableActions.Contains("move");;
	}
}

public class SelectDirection : CommandPart
{
	private readonly int _comparedToI;
	public SelectDirection(MatchConnection match, int comparedToI) : base(match) {
		_comparedToI = comparedToI;
	}
	public override bool Accepts(Command c, IGamePart o)
	{
		switch(o) {
			case HandCard:
				return false;
			case Tile tile:
				return CanAccept(c, tile);
		}
		throw new Exception("Does no accept IGamePart of type " + nameof(o));
	}

	static public int GetDirection(Tile from, Tile to) {
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

	private bool CanAccept(Command c, Tile tile) {
		if (tile.State is null) return false;
		if (tile.State?.Entity is not null) {
			if (tile.State?.Entity?.OwnerID == _match.Config.MyID)
			return false;
		}
		return GetDirection(c.Results[_comparedToI] as Tile, tile) != -1;
	}

	public override string ToActionPart(Command c, IGamePart o)
	{
		var t = o as Tile;
		var d = GetDirection(c.Results[_comparedToI] as Tile, t);
		if (d == -1) throw new Exception("Can't construct direction to " + t.Coords.ToString() + " from " + (c.Results[_comparedToI] as Tile).Coords);
		return d.ToString();
	}
}

public class SelectTileForPickTile : SelectTile {
	public SelectTileForPickTile(MatchConnection match) : base(match)
	{
	}

	protected override bool CanSelect(Command c, Tile tile)
	{
		var args = _match.State.Args;
		return args.Count == 0 || args.Contains(tile.CoordsStr);
	}
}
