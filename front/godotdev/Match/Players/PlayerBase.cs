using Godot;
using System;

public partial class PlayerBase : MarginContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}
	
	public void SetInfo(string playerName, int deckCount) {
		GetNode<Label>("ColorRect/VBoxContainer/NameLabel").Text = playerName;
		GetNode<Label>("ColorRect/VBoxContainer/DeckCountLabel").Text = "Deck count: " + deckCount;
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
//	public override void _Process(double delta)
//	{
//	}
}
