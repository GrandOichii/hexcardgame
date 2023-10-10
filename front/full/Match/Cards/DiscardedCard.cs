using core.match.states;
using Godot;
using System;

public partial class DiscardedCard : Control
{
	#region Nodes

	public Card CardNode { get; private set; }

	#endregion

	private float _defaultMinY;
	private float _cardSizeY;

	public MatchConnection Client { get; set; }

	public override void _Ready()
	{
		#region Node fetching

		CardNode = GetNode<Card>("%Card");

		#endregion
		
		_defaultMinY = CustomMinimumSize.Y;
		_cardSizeY = CardNode.Size.Y * CardNode.Scale.Y;
	}

	public void Load(MCardState card) {
		CardNode.Load(card);
	}

	#region Node connections

	private void _on_card_mouse_entered()
	{
		CreateTween().TweenProperty(this, "custom_minimum_size", new Vector2(CustomMinimumSize.X, _cardSizeY), .1f);
	}


	private void _on_card_mouse_exited()
	{
		CreateTween().TweenProperty(this, "custom_minimum_size", new Vector2(CustomMinimumSize.X, _defaultMinY), .1f);
	}

	#endregion
}



