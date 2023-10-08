using Godot;
using System;

public partial class PlayerInfo : Control
{
	private static Random _rnd = new();
	#region Packed scenes
	
	private readonly static PackedScene DiscardedCardPS = ResourceLoader.Load<PackedScene>("res://Match/Cards/DiscardedCard.tscn");
	
	#endregion

	#region Nodes

	public VBoxContainer DiscardContainerNode { get; private set; }

	#endregion

	public override void _Ready()
	{
		#region Node fetching

		DiscardContainerNode = GetNode<VBoxContainer>("%DiscardContainer");

		#endregion

		// populate discard
//		var count = _rnd.Next(10);
//		for (int i = 0; i < 10; i++) {
//			var card = DiscardedCardPS.Instantiate() as DiscardedCard;
//			DiscardContainerNode.AddChild(card);
//		}
	}
}
