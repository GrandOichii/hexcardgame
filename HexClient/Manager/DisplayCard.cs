using Godot;
using System;

namespace HexClient.Manager;

public partial class DisplayCard : Control, ICardDisplay
{
	#region Signals
	
	[Signal]
	public delegate void RightClickEventHandler(Wrapper<HexCore.Cards.ExpansionCard> card);
	
	#endregion
	
	#region Nodes
	
	public Card CardNode { get; private set; }
	private HexCore.Cards.ExpansionCard _card;

	#endregion
	
	public override void _Ready()
	{
		#region Node fetching
		
		CardNode = GetNode<Card>("%Card");
		
		#endregion

		CustomMinimumSize = CardNode.CustomMinimumSize;
	}

	public void Load(HexCore.Cards.ExpansionCard card)
	{
		_card = card;
		CardNode.Load(card);
	}

	#region Signal connections

	private void OnGuiInput(InputEvent e)
	{
		if (e.IsActionPressed("context_menu_popup"))
			EmitSignal(SignalName.RightClick, new Wrapper<HexCore.Cards.ExpansionCard>(_card));
	}

	public void SubscribeToRightClick(Action<Wrapper<HexCore.Cards.ExpansionCard>> a)
	{
		RightClick += a.Invoke;
	}

	private void OnMouseEntered()
	{
		CardNode.Focus();
	}

	private void OnMouseExited()
	{
		CardNode.Unfocus();
	}

	#endregion
}






