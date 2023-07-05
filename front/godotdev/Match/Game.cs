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

		Action = new();
	}
}
