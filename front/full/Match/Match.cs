using Godot;
using System;
using Shared;
using System.Net.Sockets;
using core.match.states;
using System.IO;
using System.Text.Json;
using core.players;

public partial class Match : Control
{
	#region Packed scenes
	
	private readonly static PackedScene HandCardPS = ResourceLoader.Load<PackedScene>("res://Match/Cards/HandCard.tscn");
	private readonly static PackedScene PlayerInfoPS = ResourceLoader.Load<PackedScene>("res://Match/Players/PlayerInfo.tscn");
	
	#endregion
	
	#region Signals
	
	[Signal]
	public delegate void StateUpdatedEventHandler(Wrapper<MatchState> stateW);
	
	#endregion
	
	#region Nodes
	
	public HBoxContainer HandContainerNode { get; private set; }
	public VBoxContainer PlayerContainerNode { get; private set; }
	public Control CommandContainerNode { get; private set; }
	public RichTextLabel LogsLabelNode { get; private set; }
	public LineEdit CommandLineNode { get; private set; }
	public Timer PollStateTimerNode { get; private set; }
	
	#endregion
	
	private MatchConnection _client;
	private NetworkStream _stream;
	private MatchState _state;
	private bool _fixedPOrder = false;
	private MatchInfoState _config;

	public override void _Ready()
	{
		#region Node fetching
		
		HandContainerNode = GetNode<HBoxContainer>("%HandContainer");
		PlayerContainerNode = GetNode<VBoxContainer>("%PlayerContainer");
		CommandContainerNode = GetNode<Control>("%CommandContainer");
		LogsLabelNode = GetNode<RichTextLabel>("%LogsLabel");
		CommandLineNode = GetNode<LineEdit>("%CommandLine");
		PollStateTimerNode = GetNode<Timer>("%PollStateTimer");

		#endregion
		
		// populate hand
//		for (int i = 0; i < 10; i++) {
//			var card = HandCardPS.Instantiate() as HandCard;
//			HandContainerNode.AddChild(card);
//		}
	}
	
	public void Load(Wrapper<MatchConnection> connection) {
		_client = connection.Value;
		_stream = _client.GetStream();

		// read configuration
		var message = NetUtil.Read(_stream);
		_config = MatchInfoState.FromJson(message);
		var pCount = _config.PlayerCount;
		
		foreach (var child in PlayerContainerNode.GetChildren())
			child.Free();
		
		for (int i = 0; i < pCount; i++) {
			var child = PlayerInfoPS.Instantiate() as PlayerInfo;
			PlayerContainerNode.AddChild(child);
			child.PlayerI = i;
		}

		_client.ReceiveTimeout = 20;

		PollStateTimerNode.Start();
		// TODO

	}

	private void LoadState(MatchState state) {
		// _client.LastState = state;
		_state = state;
		EmitSignal(SignalName.StateUpdated, new Wrapper<MatchState>(state));

		UpdatePlayers();
		UpdateHand();
	}

	#region State update methods

	private void UpdateHand() {
		var cCount = HandContainerNode.GetChildCount();
		var nCount = _state.MyData.Hand.Count;

		if (nCount > cCount) {
			// fill hand up to new count
			for (int i = 0; i < nCount - cCount; i++) {
				var child = HandCardPS.Instantiate() as HandCard;
				HandContainerNode.AddChild(child);
			}
		}
		if (nCount < cCount) {
			// trim child count
			for (int i = nCount + 1; i < cCount; i++) {
				var child = HandContainerNode.GetChild(i);
				child.QueueFree();
			}
		}
		// load card data
		for (int i = 0; i < nCount; i++) {
			(HandContainerNode.GetChild(i) as HandCard).Load(_state.MyData.Hand[i]);
		}
	}

	private void UpdatePlayers() {
		if (!_fixedPOrder) {
			var myI = _config.MyI;
			var pCount = _config.PlayerCount;
			for (int i = 0; i < pCount; i++) {
				var pNode = PlayerContainerNode.GetChild(i) as PlayerInfo;
				pNode.PlayerI = (i + myI + 1) % pCount;
			}
			_fixedPOrder = true;
		}

		foreach (var child in PlayerContainerNode.GetChildren()) {
			switch (child) {
			case PlayerInfo pInfo:
				pInfo.UpdateState(_state);
				break;
			default:
				break;
			}
		}
	}
	
	#endregion

	
	#region Signal connections

	private void _on_match_hub_new_match_connection(Wrapper<MatchConnection> connection)
	{
		Load(connection);
	}

	private void _on_submit_command_button_pressed()
	{
		NetUtil.Write(_client.GetStream(), CommandLineNode.Text);
		CommandLineNode.Text = "";
	}

	private void _on_poll_state_timer_timeout()
	{
//		GD.Print("poll");
		try {
			var message = NetUtil.Read(_stream);
			GD.Print(message);
			var state = MatchState.FromJson(message);
			LoadState(state);
		} catch (IOException) { return; }
	}
	
	#endregion
}
