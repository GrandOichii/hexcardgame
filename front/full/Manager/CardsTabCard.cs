using Godot;
using System;

public partial class CardsTabCard : Control
{
	#region Signals
	
	[Signal]
	public delegate void CardEditRequestedEventHandler(Wrapper<CardData> card);
	
	#endregion
	
	#region Nodes
	
	public Card CardNode { get; private set; }
	public Label ExpansionLabelNode { get; private set; }
	
	#endregion
	
	private CardData _card;
	
	private int _expansionI;
	private int ExpansionI {
		get => _expansionI;
		set {
			_expansionI = value;
			var l = _card.Expansions.Count;
			if (value >= l) _expansionI = 0;
			if (value < 0) _expansionI = l - 1;

			ExpansionLabelNode.Text = _card.Expansions[_expansionI].Name;
		}
	}
	
	public override void _Ready()
	{ 
		#region Node fetching
		
		CardNode = GetNode<Card>("%Card");
		ExpansionLabelNode = GetNode<Label>("%ExpansionLabel");
		
		#endregion
		
//		CardNode.CustomMinimumSize = CardNode.CustomMinimumSize;
	}
	
	public void Load(CardData card) {
		_card = card;
		CardNode.Load(card);
		ExpansionI = 0;
	}
	
	public void Edit() {
		EmitSignal(SignalName.CardEditRequested, new Wrapper<CardData>(_card));
	}
	
	#region Signal connections

	
	private void _on_card_mouse_entered()
	{
		CardNode.Focus();
	}

	private void _on_card_mouse_exited()
	{
		CardNode.Unfocus();
	}

	private void _on_card_gui_input(InputEvent e)
	{
		if (e.IsActionPressed("manager-edit-card"))
			Edit();
	}

	private void _on_scroll_left_button_pressed()
	{
		--ExpansionI;
	}

	private void _on_scroll_right_button_pressed()
	{
		++ExpansionI;
	}

	#endregion
}




