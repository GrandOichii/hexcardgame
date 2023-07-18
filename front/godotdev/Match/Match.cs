using Godot;
using System;
using Shared;
using System.Collections.Generic;
using core.match.states;
using System.IO;


public partial class Match : Node2D
{
	static readonly PackedScene CardBaseScene = ResourceLoader.Load("res://Match/Cards/HandCardBase.tscn") as PackedScene;
	static readonly PackedScene HoverCardBaseScene = ResourceLoader.Load("res://Match/Cards/HoverCardBase.tscn") as PackedScene;	
	static readonly PackedScene PlayerBaseScene = ResourceLoader.Load("res://Match/Players/PlayerBase.tscn") as PackedScene;
	static readonly PackedScene EntityBaseScene = ResourceLoader.Load("res://Match/Tiles/EntityBase.tscn") as PackedScene;
	
	private VBoxContainer PlayerContainer;
	private HBoxContainer HandContainer;
	private Node2D EntitiesNode;
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
		EntitiesNode = MapContainer.GetNode<Node2D>("%Entities");
		
		// read match info
		var message = NetUtil.Read(Game.Instance.Client.GetStream());
		Game.Instance.Client.ReceiveTimeout = 20;
		var info = MatchInfoState.FromJson(message);
		Game.Instance.MatchInfo = info;
		var playerCount = info.PlayerCount;

		var myI = info.MyI;
		for (int i = 0; i < playerCount; i++) {
			var pNode = PlayerBaseScene.Instantiate() as PlayerBase;
			// pNode._Ready();
			pNode.PlayerI = (i + myI + 1) % playerCount;
			PlayerContainer.AddChild(pNode);
			Players.Add(pNode);
		}
		
	}

	
	public void LoadState(MatchState state) {
		Game.Instance.LoadState(state);
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

		// check entities
		var tiles = state.Map.Tiles;
		var newEn = new Dictionary<string, MCardState>();
		var newPos = new Dictionary<string, int[]>();
		for (int i = 0; i < tiles.Count; i++) {
			var row = tiles[i];
			for (int j = 0; j < row.Count; j++) {
				var tile = row[j];
				if (tile is null || tile?.Entity is null) continue;
				var en = (MCardState)(tile?.Entity);
				newEn.Add(en.MID, en);
				newPos.Add(en.MID, new int[]{i, j});
			}
		}
		
		var removedEntities = Difference(Game.Instance.CurrentEntities, newEn);
		var addedEntities = Difference(newEn, Game.Instance.CurrentEntities);
		var sameKeys = SameKeys(newEn, Game.Instance.CurrentEntities);
		
		// remove old entities
		for (int i = EntitiesNode.GetChildCount() - 1; i >= 0; i--) {
			var child = EntitiesNode.GetChild<EntityBase>(i);
			var mid = child.LastState.MID;
			if (!removedEntities.ContainsKey(mid)) continue;

			var pos = Game.Instance.CurrentPositions[mid];
			var t = MapContainer.Tiles[pos[0]][pos[1]];
			t.Entity = null;

			GD.Print("Removing " + child.LastState.ID + " " + child.LastState.MID + " at " + pos[0] + "." + pos[1]);
			EntitiesNode.RemoveChild(child);
			child.Free();

		}
		// add new entities
		foreach (var pair in addedEntities) {
			var mid = pair.Key;
			var entity = pair.Value;
			var eNode = EntityBaseScene.Instantiate() as EntityBase;
			EntitiesNode.AddChild(eNode);
			eNode.Load(entity);

			var loc = newPos[mid];
			var t = MapContainer.Tiles[loc[0]][loc[1]];
			// eNode.Position = t.GlobalPosition;
			eNode.Position = new(t.Position.X, t.Position.Y);
			t.Entity = eNode;
		}
		foreach (var mid in sameKeys) {
			var newP = newPos[mid];
			var oldP = Game.Instance.CurrentPositions[mid];
			var t = MapContainer.Tiles[oldP[0]][oldP[1]];
			var e = t.Entity;
			if (!(newP[0] == oldP[0] && newP[1] == oldP[1])) {
				t.Entity = null;
				var newT = MapContainer.Tiles[newP[0]][newP[1]];
				newT.Entity = e;
				e.CreateTween().TweenProperty(e, "position", newT.Position, .1);
			}
			e.Load(newEn[mid]);
		}
		// move existing entities

		foreach (var key in addedEntities.Keys) {
			GD.Print("NEW " + key);
		}
		foreach (var key in removedEntities.Keys) {
			GD.Print("REMOVED " + key);
		}
		foreach (var mid in sameKeys) {
			var newP = newPos[mid];
			var oldP = Game.Instance.CurrentPositions[mid];
			if (newP[0] == oldP[0] && newP[1] == oldP[1]) continue;

			GD.Print("MOVED " + mid + " " + oldP[0] + "." + oldP[1] + " -> " + newP[0] + "." + newP[1]);
		}

		Game.Instance.CurrentEntities = newEn;
		Game.Instance.CurrentPositions = newPos;		
	}

	private static List<string> SameKeys(Dictionary<string, MCardState> first, Dictionary<string, MCardState> second) {
		var result = new List<string>();
		foreach (var key in first.Keys)
			if (second.ContainsKey(key)) result.Add(key);
		return result;
	}
	
	private static Dictionary<string, MCardState> Difference(Dictionary<string, MCardState> first, Dictionary<string, MCardState> second) {
		var result = new Dictionary<string, MCardState>();
		foreach (var key in first.Keys) {
			if (second.ContainsKey(key)) continue;
			result.Add(key, first[key]);
		}
		return result;
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
				var cNode = CardBaseScene.Instantiate() as HandCardBase;
				HandContainer.AddChild(cNode);
			}
		}
		HandCount = cards.Count;
		for (int i = 0; i < HandCount; i++) {
			var cNode = HandContainer.GetChild(i) as HandCardBase;
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



