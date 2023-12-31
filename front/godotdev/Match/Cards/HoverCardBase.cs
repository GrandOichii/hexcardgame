using Godot;
using System;

using core.match.states;

public partial class HoverCardBase : Node2D
{
	private CardBase CardNode;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CardNode = GetNode("%Card") as CardBase;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Position = GetViewport().GetMousePosition();
	}
	
	public void Load(string cID) {
		var card = CardFetcher.Instance.Get(cID);
		
		CardNode.Load(card);
	}

	public void Load(MCardState state) {
		CardNode.Load(state);
	}
}
