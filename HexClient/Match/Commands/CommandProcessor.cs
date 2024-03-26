using System.Collections.Generic;
using Godot;
using HexCore.GameMatch.States;

namespace HexClient.Match.Commands;

public interface IGamePart {}

public class CommandProcessor {
	private readonly Dictionary<string, List<Command>> _requestCommandMap;
	private BaseState _state;
	public BaseState State { 
		get => _state; 
		set {
			_state = value;
			_commands = _requestCommandMap[_state.Request];
		}
	}
	public MatchInfoState Config { get; set; }

	#nullable enable
	public Command? CurrentCommand { get; set; }
	#nullable disable

	private List<Command> _commands = new();

	// public HoverCard HoverCard { get; set; }
	private readonly IConnection _connection;

	public CommandProcessor(IConnection connection) : base() {
		_connection = connection;

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
					part.Process(command, o, _connection);
					return;
				}
			}
			// TODO? throw exception
			return;
		}

		CurrentCommand.Parts[CurrentCommand.PartI].Process(CurrentCommand, o, _connection);
	}

	public void ResetCommand() {
		CurrentCommand?.Reset();
	}
	
}
