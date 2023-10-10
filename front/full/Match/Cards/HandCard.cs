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

	public override void _Input(InputEvent e)
	{
		if (e.IsActionPressed("cancel-command"))
			Recheck();
	}

	public void Recheck() {
		CardNode.Unfocus();
		// _on_card_mouse_entered();
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
		Recheck();
	}

	private void _on_card_mouse_entered()
	{
		// GD.Print("EE");
		if (!Client.Accepts(this)) return;
		CardNode.Focus();
	}

	private void _on_card_mouse_exited()
	{
		CardNode.Unfocus();
	}
	
	#endregion
}

