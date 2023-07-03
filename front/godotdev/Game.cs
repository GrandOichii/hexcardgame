using Godot;
using System;
using System.Net.Sockets;

public class Game
{
	public TcpClient Client { get; }
	static public Game Instance { get; } = new Game();
	
	private Game() {
		Client = new();
	}
}
