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
	
	// private MatchState _lastState;
	// public MatchState LastState {
	// 	get => _lastState;
	// 	set {
	// 		_lastState = value;
	// 	}
	// }
	
}
