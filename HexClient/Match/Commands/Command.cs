using System;
using System.Collections.Generic;
using Godot;
using HexClient.Match.Grid;
using HexCore.GameMatch.States;

namespace HexClient.Match.Commands;


abstract public class CommandPart {
	abstract public bool Accepts(Command c, IGamePart o);
	abstract public string ToActionPart(Command c, IGamePart o);
	public void Process(Command c, IGamePart part, IConnection connection) {
		c.Add(part, connection);
	}
	protected CommandProcessor _processor;
	public CommandPart(CommandProcessor processor) {
		_processor = processor;
	}
}

public class Command {
	public string Name { get; protected set; }
	public List<CommandPart> Parts { get; protected set; }
	public int PartI => Results.Count;
	public List<IGamePart> Results { get; } = new();
	private readonly CommandProcessor _processor;
	
	public Command(CommandProcessor processor, string name, List<CommandPart> parts) {
		_processor = processor;
		Name = name;
		Parts = parts;
	}

	public void Reset() {
		Results.Clear();
		_processor.CurrentCommand = null;
	}

	public void Add(IGamePart part, IConnection connection) {
		Results.Add(part);
		if (PartI != Parts.Count) return;

		var words = new string[PartI];
		for (int i = 0; i < Results.Count; i++) {
			words[i] = Parts[i].ToActionPart(this, Results[i]);
		}
		var message = Name + " " + words.Join(" ");
		connection.Write(message);
		GD.Print("Wrote ", message);
		Reset();
	}
}
