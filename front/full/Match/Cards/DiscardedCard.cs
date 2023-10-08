using Godot;
using System;

public partial class DiscardedCard : Control
{
	#region Nodes

	public Card CardNode { get; private set; }

	#endregion

	private float _defaultMinY;
	private float _cardSizeY;

	public override void _Ready()
	{
		#region Node fetching

		CardNode = GetNode<Card>("%Card");

		#endregion
		
		_defaultMinY = CustomMinimumSize.Y;
		_cardSizeY = CardNode.Size.Y * CardNode.Scale.Y;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
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



