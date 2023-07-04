using Godot;
using System;

using core.match.states;

public partial class PlayerBase : MarginContainer
{
	private Label NameLabel;
	private Label DeckCountLabel;
	private Label EnergyLabel;
	
	public int PlayerI { get; set; }
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		NameLabel = GetNode<Label>("%NameLabel");
		DeckCountLabel = GetNode<Label>("%DeckCountLabel");
		EnergyLabel = GetNode<Label>("%EnergyLabel");
		
	}
	
	public void Load(ref MatchState state) {
		var playerState = state.Players[PlayerI];
		
		NameLabel.Text = playerState.Name;
		EnergyLabel.Text = "Energy: " + playerState.Energy;
		DeckCountLabel.Text = "Deck count: " + playerState.DeckCount;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
//	public override void _Process(double delta)
//	{
//	}
}
