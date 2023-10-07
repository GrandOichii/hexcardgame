using Godot;
using System;

public partial class Match : Control
{
	#region Packed scenes
	
	private readonly static PackedScene CardPS = ResourceLoader.Load<PackedScene>("res://Match/Cards/Card.tscn");
	
	#endregion
	
	#region Nodes
	
	public HBoxContainer HandContainerNode { get; private set; }
	
	#endregion
	
	public override void _Ready()
	{
		#region Node fetching
		
		HandContainerNode = GetNode<HBoxContainer>("%HandContainer");
		
		#endregion
		
		// populate hand
		for (int i = 0; i < 10; i++) {
			var card = CardPS.Instantiate() as Card;
			HandContainerNode.AddChild(card);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
