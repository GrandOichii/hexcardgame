using Godot;
using System;

using core.cards;
using core.match.states;

public partial class HandCardBase : Control, IGamePart
{
	// TODO with cardbase input processing disabled, can't scroll card text
	
	// nodes
	public CardBase CardNode { get; private set; }
//	private CardBase UpperCardNode;

	private Color BaseColor;
	private Color HighlightColor = new("blue");
	
	public override void _Ready()
	{
		CardNode = GetNode<CardBase>("%Card");
//		UpperCardNode = GetNode<CardBase>("%UpperCard");
		
		BaseColor = CardNode.BGColor;
		CustomMinimumSize = CardNode.MainCardNode.CustomMinimumSize;
		GD.Print(CustomMinimumSize);
		GetNode<CollisionShape2D>("%CollisionShape").Set("shape/size", CardNode.MainCardNode.CustomMinimumSize);
		
		// Load(CardFetcher.Instance.Get("dev::Urakshi Raider"));
	}

	private void SetColor(Color c) {
		// CardNode.Bg.Color = HighlightColor;
//		CardNode.BgNode.Set("theme_override/panel/bg_color", c);
//		CardNode.BGColor = c;
		CreateTween().TweenProperty(CardNode, "BGColor", c, .1f);
	}
	
	private void OnCollisionMouseEntered()
	{
		if (!Game.Instance.Accepts(this)) return;
		
		SetColor(HighlightColor);
	}
	
	private void OnCollisionMouseExited()
	{
//		UpperCardNode.Visible = false;
		SetColor(BaseColor);
	}
	
	public void Load(Card card) {
//		UpperCardNode.Load(card);
		CardNode.Load(card);
	}
	
	public void Load(MCardState card) {
//		UpperCardNode.Load(card);
		CardNode.Load(card);
	}
	
	private void OnGuiInput(Node viewport, InputEvent @event, long shape_idx)
	{
		if (@event is InputEventMouseButton) {
			var e = @event as InputEventMouseButton;
			if (e.IsPressed() && e.ButtonIndex == MouseButton.Left) {
				if (!Game.Instance.Accepts(this)) return;
				Game.Instance.Process(this);
				OnCollisionMouseExited();

				// if (CardNode.LastState.AvailableActions.Count == 0) return;
				
				// var game = Game.Instance;
				// var action = game.Action;
				// if (action.Count == 0)  {
				// 	GD.Print(CardNode.LastState.MID);
				// 	game.AddToAction("play");
				// 	game.AddToAction(CardNode.LastState.MID);
				// 	return;
				// }
			}
		}
		// Replace with function body.
	}

	public void AddTo(HBoxContainer c) {
		c.AddChild(this);
		CardNode.GlobalPosition = new(CardNode.GlobalPosition.X, CardNode.GlobalPosition.Y - 200);
//		CardNode.Position = new(GetWindow().Size.X / 2, GetWindow().Size.Y / 2);
		CreateTween().TweenProperty(CardNode, "position", Position, .2f);
	}

	// public string ToActionPart(Command command)
	// {
	// 	// TODO? more complex behavior
	// 	return CardNode.LastState.MID;
	// }

}


