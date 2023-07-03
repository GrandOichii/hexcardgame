using Godot;
using System;
using Shared;
using System.Collections.Generic;
using core.match.states;

public partial class Match : Node2D
{
	static PackedScene CardBaseScene = ResourceLoader.Load("res://Match/Cards/CardBase.tscn") as PackedScene;
	static PackedScene PlayerBaseScene = ResourceLoader.Load("res://Match/Players/PlayerBase.tscn") as PackedScene;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var message = NetUtil.Read(Game.Instance.Client.GetStream());
		var info = MatchInfoState.FromJson(message);
		var playerCount = info.PlayerCount;


		var pNodeContainer = GetNode<VBoxContainer>("UI/HBox/Players");
		for (int i = 0; i < playerCount; i++) {
			var pNode = PlayerBaseScene.Instantiate() as PlayerBase;
			pNodeContainer.AddChild(pNode);
			pNode.SetInfo("P" + i.ToString(), i);
//			GD.Print(i);
		}

//		OS.Alert(message);
//		List<string> cards = new() {
//			"dev::Castle",
//			"dev::Mage Initiate",
//			"dev::Flame Eruption",
//			"dev::Castle",
//			"dev::Mage Initiate",
//			"dev::Flame Eruption",
//			"dev::Castle",
//			"dev::Mage Initiate",
//			"dev::Flame Eruption",
//			"dev::Castle",
//			"dev::Mage Initiate",
//			"dev::Flame Eruption",
//			"dev::Castle",
//			"dev::Mage Initiate",
//			"dev::Flame Eruption",
//		};
		// var cardType = ResourceLoader.Load<CardBase>("Cards/CardBase.tscn");
		
		message = NetUtil.Read(Game.Instance.Client.GetStream());
		var state = MatchState.FromJson(message);
		var cards = state.MyData.MyHand;
		
		var node = GetNode<HBoxContainer>("UI/HBox/CenterBox/CardsScroll/Cards");
//		node.Scale *= new Vector2(.5f, .5f);
		foreach (var card in cards) {
			// MCardState card;
			// card.ID = cID;
			// card.AvaliableActions = new();
			// card.MID = "12";
			// card.Modifications = new();
			// card.OwnerID = "1";

			// var cNode = new CardBase();
			var cNode = CardBaseScene.Instantiate() as CardBase;
			// cNode.Scale = new(.1f, .1f);
//			cNode.SetSize(new(cNode.Size.X*.1f, cNode.Size.Y*.1f));

			cNode.Load(card);

			node.AddChild(cNode);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
//	public override void _Process(double delta)
//	{
//	}
}
