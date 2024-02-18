using Godot;
using System;

public partial class HoverCard : Control
{
	#region Nodes
	
	public Card CardNode { get; private set; }
	
	#endregion

	// private MatchConnection _client;
	// public MatchConnection Client {
	// 	get => _client;
	// 	set {
	// 		_client = value;
	// 		CardNode.Client = _client;
	// 	}
	// }
	
	public override void _Ready()
	{
		#region Node fetching
		
		CardNode = GetNode<Card>("%Card");
		
		#endregion
		
		CustomMinimumSize = CardNode.Size * CardNode.Scale;
	}
	
	public override void _Process(double delta) {
//		CustomMinimumSize = CardNode.Size * CardNode.Scale;
	}
	
	public void Load(MatchCardState card) {
		CardNode.Load(card);
	}
	
}
