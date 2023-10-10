using core.match.states;
using Godot;
using System;

public partial class HandCard : Control, IGamePart
{
	#region Nodes
	
	public Card CardNode { get; private set;}
	
	#endregion
	
	private MatchConnection _client;
	public MatchConnection Client {
		get => _client; 
		set {
			_client = value;
			CardNode.Client = value;
		}
	}

	public override void _Ready()
	{
		#region Node fetching

		CardNode = GetNode<Card>("%Card");
		
		#endregion

		CustomMinimumSize = CardNode.Size * CardNode.Scale;
	}

	public void Load(MCardState card) {
		CardNode.Load(card);
	}
	
	#region Signal connections

	private void _on_card_add_to_action()
	{
//		GD.Print("clicked");
		if (!Client.Accepts(this)) return;
		Client.Process(this);
		CardNode.Unfocus();
	}

	private void _on_card_mouse_entered()
	{
		if (!Client.Accepts(this)) return;
		CardNode.Focus();
	}

	private void _on_card_mouse_exited()
	{
		CardNode.Unfocus();
	}
	
	#endregion
}

