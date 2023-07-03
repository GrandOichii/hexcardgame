using core.match.states;
using Godot;
using System;

public partial class CardBase : MarginContainer
{
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// OS.("Hello, world");
		MCardState card;
		card.MID = "dev::Castle";
		card.AvaliableActions = new();
		card.ID = "12";
		card.Modifications = new();
		card.OwnerID = "1";
		Load(card);
	}
	
	public void Load(MCardState card) {
		
	}

//	// Called every frame. 'delta' is the elapsed time since the previous frame.
//	public override void _Process(double delta)
//	{
//	}
}
