using Godot;
using System;
using Shared;
using System.Collections.Generic;
using core.match.states;
using System.IO;


public partial class Match : Node2D
{
	static readonly PackedScene CardBaseScene = ResourceLoader.Load("res://Match/Cards/CardBase.tscn") as PackedScene;
	static readonly PackedScene HoverCardBaseScene = ResourceLoader.Load("res://Match/Cards/HoverCardBase.tscn") as PackedScene;	
	static readonly PackedScene PlayerBaseScene = ResourceLoader.Load("res://Match/Players/PlayerBase.tscn") as PackedScene;
	
	private VBoxContainer PlayerContainer;
	private HBoxContainer HandContainer;
	private Map MapContainer;
	
	private Logs LogsContainer;
	
	private List<PlayerBase> Players = new();
	private int HandCount = 0;

	private HoverCardBase HoverCard;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		HoverCard = HoverCardBaseScene.Instantiate() as HoverCardBase;
		Game.Instance.HoverCard = HoverCard;
		HoverCard.Visible = false;
		HoverCard._Ready();
		AddChild(HoverCard);

		PlayerContainer = GetNode<VBoxContainer>("%Players");
		HandContainer = GetNode<HBoxContainer>("%Cards");
		LogsContainer = GetNode<Logs>("%Logs");
		MapContainer = GetNode<Map>("%Map");
		
		var message = NetUtil.Read(Game.Instance.Client.GetStream());
		Game.Instance.Client.ReceiveTimeout = 20;
		var info = MatchInfoState.FromJson(message);
		Game.Instance.MatchInfo = info;
		var playerCount = info.PlayerCount;

		var myI = info.MyI;
		for (int i = 0; i < playerCount; i++) {
			var pNode = PlayerBaseScene.Instantiate() as PlayerBase;
			// TODO is this ok?
			pNode._Ready();
			pNode.PlayerI = (i + myI + 1) % playerCount;
			PlayerContainer.AddChild(pNode);
			Players.Add(pNode);
		}
		
	}

	
	public void LoadState(MatchState state) {
		Game.Instance.LastState = state;
		foreach (var player in Players) {
			player.Load(ref state);
		}
		
		// load new logs
		var newLogs = state.NewLogs;
		
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
			LogsContainer.AppendText(message);
		}
		
		// load hand
		LoadHand(state.MyData.MyHand);
		
		// load map
		MapContainer.Load(state.Map);
	}
	
	private void LoadHand(List<MCardState> cards) {		
		if (cards.Count < HandCount) {
			for (int i = HandCount - 1; i >= cards.Count; i--) {
				// Hand.RemoveAt(i);
				var child = HandContainer.GetChild(i);
				HandContainer.RemoveChild(child);
				child.Free();
			}
		}
		if (cards.Count > HandCount) {
			for (int i = 0; i < cards.Count - HandCount; i++) {
				var cNode = CardBaseScene.Instantiate() as CardBase;
				//TODO is this ok?
				cNode._Ready();
	//			cNode.Notification(NotificationReady);
				// cNode.Scale = new(.1f, .1f);
	//			cNode.SetSize(new(cNode.Size.X*.1f, cNode.Size.Y*.1f));

//				cNode.Load(card);

				HandContainer.AddChild(cNode);
			}
		}
		HandCount = cards.Count;
		for (int i = 0; i < HandCount; i++) {
			var cNode = HandContainer.GetChild(i) as CardBase;
			cNode.Load(cards[i]);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	
	private void _OnPollLogsTimerTimeout()
	{
		try {
			var message = NetUtil.Read(Game.Instance.Client.GetStream());
			var state = MatchState.FromJson(message);
			LoadState(state);
		} catch (IOException) { return; }
		
	}
	
	private void OnLogsMetaHoverStarted(Variant meta)
	{
		HoverCard.Visible = true;
		
		HoverCard.Load(meta.AsString());
	}
	private void OnLogsMetaHoverEnded(Variant meta)
	{
		HoverCard.Visible = false;
		
	}
	private void OnPassButtonPressed()
	{
		var stream = Game.Instance.Client.GetStream();
		NetUtil.Write(stream, "pass");
		// Replace with function body.
	}
}



