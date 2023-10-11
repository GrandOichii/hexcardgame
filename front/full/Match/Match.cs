using Godot;
using System;
using Shared;
using System.Net.Sockets;
using core.match.states;
using System.IO;
using System.Text.Json;
using core.players;
using System.Threading.Tasks;

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
	public Grid GridNode { get; private set; }
	public HoverCard HoverCardNode { get; private set; }
	
	#endregion
	
	public MatchConnection Client { get; private set; }
	private NetworkStream _stream;
	public MatchState State { get; private set; }
	private bool _fixedPOrder = false;
	public MatchInfoState Config { get; private set; }

	public override void _Ready()
	{
		#region Node fetching
		
		HandContainerNode = GetNode<HBoxContainer>("%HandContainer");
		PlayerContainerNode = GetNode<VBoxContainer>("%PlayerContainer");
		CommandContainerNode = GetNode<Control>("%CommandContainer");
		LogsLabelNode = GetNode<RichTextLabel>("%LogsLabel");
		CommandLineNode = GetNode<LineEdit>("%CommandLine");
		PollStateTimerNode = GetNode<Timer>("%PollStateTimer");
		GridNode = GetNode<Grid>("%Grid");
		HoverCardNode = GetNode<HoverCard>("%HoverCard");

		#endregion
		
//		HandContainerNode.CustomMinimumSize = new(0, HandCardPS.Instantiate<HandCard>().CardNode.CustomMinimumSize.Y);
	}
	
	public override void _Input(InputEvent e) {
		if (e.IsActionPressed("cancel-command"))
			CancelCommand();
	}
	
	private void CancelCommand() {
		Client.ResetCommand();
	}
	
	public void Load(Wrapper<MatchConnection> connection) {
		Client = connection.Value;
		_stream = Client.GetStream();

		// read configuration
		Task.Run(() => {
			var message = NetUtil.Read(_stream);
			Config = MatchInfoState.FromJson(message);
			CallDeferred("LoadConfiguration");
		});
	}
	
	private void LoadConfiguration()
	{
		Client.Config = Config;
		Client.HoverCard = HoverCardNode;
		GridNode.Client = Client;
		var pCount = Config.PlayerCount;
		
		foreach (var child in PlayerContainerNode.GetChildren())
			child.Free();
		
		for (int i = 0; i < pCount; i++) {
			var child = PlayerInfoPS.Instantiate() as PlayerInfo;
			PlayerContainerNode.AddChild(child);
			child.Client = Client;
			child.PlayerI = i;
		}

		Client.ReceiveTimeout = 20;

		PollStateTimerNode.Start();
	}

	private void LoadState(MatchState state) {
		Client.State = state;
		State = state;
		EmitSignal(SignalName.StateUpdated, new Wrapper<MatchState>(state));

		UpdatePlayers();
		UpdateHand();
		GridNode.Load(state);
		UpdateLogs();
	}

	#region State update methods

	private void UpdateHand() {
		var cCount = HandContainerNode.GetChildCount();
		var nCount = State.MyData.Hand.Count;

		if (nCount > cCount) {
			// fill hand up to new count
			for (int i = 0; i < nCount - cCount; i++) {
				var child = HandCardPS.Instantiate() as HandCard;
				HandContainerNode.AddChild(child);
				child.Client = Client;
			}
		}
		if (nCount < cCount) {
			// trim child count
			for (int i = cCount - 1; i >= nCount; i--) {
			// for (int i = nCount + 1; i < cCount; i++) {
				var child = HandContainerNode.GetChild(i);
				child.Free();
			}
		}
		// load card data
		for (int i = 0; i < nCount; i++) {
			(HandContainerNode.GetChild(i) as HandCard).Load(State.MyData.Hand[i]);
		}
	}

	private void UpdatePlayers() {
		if (!_fixedPOrder) {
			var myI = Config.MyI;
			var pCount = Config.PlayerCount;
			for (int i = 0; i < pCount; i++) {
				var pNode = PlayerContainerNode.GetChild(i) as PlayerInfo;
				pNode.PlayerI = (i + myI + 1) % pCount;
			}
			_fixedPOrder = true;
		}

		foreach (var child in PlayerContainerNode.GetChildren()) {
			switch (child) {
			case PlayerInfo pInfo:
				pInfo.UpdateState(State);
				break;
			default:
				break;
			}
		}
	}

	public void UpdateLogs() {
		var newLogs = State.NewLogs;
		
		foreach (var log in newLogs) {
			var message = "- ";
			foreach (var part in log) {
				var text = part.Text;
				if (part.CardRef.Length > 0) {
					text = "[color=red][url=" + part.CardRef + "]" + text + "[/url][/color]";
				}
				message += text + " ";
			}
			message += "\n";
			LogsLabelNode.AppendText(message);
		}
	}
	
	#endregion
	
	public void Write(string command) {
		NetUtil.Write(_stream, command);
	}

	
	#region Signal connections

	private void _on_match_hub_new_match_connection(Wrapper<MatchConnection> connection)
	{
		Load(connection);
	}

	private void _on_submit_command_button_pressed()
	{
		Write(CommandLineNode.Text);
		CommandLineNode.Text = "";
	}

	private void _on_poll_state_timer_timeout()
	{
//		GD.Print("poll");
		try {
			var message = NetUtil.Read(_stream);
			var state = MatchState.FromJson(message);
			LoadState(state);
		} catch (IOException) { return; }
	}

	private void _on_pass_button_pressed()
	{
		Client.ResetCommand();
		Write("pass");
	}

	private void _on_logs_label_meta_hover_ended(Variant meta)
	{
		GD.Print(meta.AsString());
		// Replace with function body.
	}


	private void _on_logs_label_meta_hover_started(Variant meta)
	{
		// Replace with function body.
	}
	
	#endregion
}

