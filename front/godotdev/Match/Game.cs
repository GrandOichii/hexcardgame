using core.match.states;
using Godot;
using System;
using System.Net.Sockets;

public class Game
{
	public TcpClient Client { get; }
	static public Game Instance { get; } = new Game();

	public MatchInfoState MatchInfo { get; set; }
	public MatchState LastState { get; set; }
	
	private Game() {
		Client = new();
	}
}
