using Godot;
using System;

using core.cards;
using core.match.states;

public partial class HandCardBase : MarginContainer
{
	// TODO with cardbase input processing disabled, can't scroll card text
	
	// nodes
	private CardBase CardNode;
//	private CardBase UpperCardNode;

	private Color BaseColor;
	private Color HighlightColor = new Color("blue");
	
	public override void _Ready()
	{
		CardNode = GetNode<CardBase>("%Card");
//		UpperCardNode = GetNode<CardBase>("%UpperCard");
		
		BaseColor = CardNode.Bg.Color;
		
		Load(CardFetcher.Instance.Get("dev::Urakshi Raider"));
	}
	
	private void OnCollisionMouseEntered()
	{
//		UpperCardNode.Visible = true;
		CardNode.Bg.Color = HighlightColor;
	}
	
	private void OnCollisionMouseExited()
	{
//		UpperCardNode.Visible = false;
		CardNode.Bg.Color = BaseColor;
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
				var game = Game.Instance;
				var action = game.Action;
				if (action.Count == 0)  {
					GD.Print(CardNode.LastState.MID);
					game.AddToAction("play");
					game.AddToAction(CardNode.LastState.MID);
					return;
				}
			}
		}
		// Replace with function body.
	}
}


