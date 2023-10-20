using Godot;
using System;

public partial class DeckEditWindow : Window
{
	#region Packed scenes
	
	private readonly static PackedScene DeckCardPS = ResourceLoader.Load<PackedScene>("res://Manager/DeckCard.tscn");
	
	#endregion

	#region Nodes
	
	public Window AddCardWindowNode { get; private set; }
	public LineEdit NameEditNode { get; private set; }
	public TextEdit DescriptionEditNode { get; private set; }
	public VBoxContainer CardsContainerNode { get; private set; }
	
	#endregion
	
	private DeckData _current;
	
	public override void _Ready()
	{
		#region Node fethching
		
		NameEditNode = GetNode<LineEdit>("%NameEdit");
		DescriptionEditNode = GetNode<TextEdit>("%DescriptionEdit");
		CardsContainerNode = GetNode<VBoxContainer>("%CardsContainer");
		AddCardWindowNode = GetNode<Window>("%AddCardWindow");
		
		#endregion
	}
	
	public void Load(DeckData? deck) {		
		_current = deck;
		
		var d = deck;
		if (deck is null) d = new();
		
		NameEditNode.Text = d.Name;
		
		// TODO add back
//		DescriptionEditNode.Text = d.Description

		// add cards
		foreach (var card in d.Cards) {
			var child = DeckCardPS.Instantiate() as DeckCard;
			CardsContainerNode.AddChild(child);

			child.Load(card);
		}

		Show();
	}
	
	#region Signal connections
	
	private void _on_close_requested()
	{
		Hide();
	}

	private void _on_add_button_pressed()
	{
		// TODO
	}

	private void _on_name_filter_text_changed(string new_text)
	{
		foreach (var child in CardsContainerNode.GetChildren()) {
			var c = child as DeckCard;
			var card = c.Data;
			c.Visible = card.Card.Card.Name.ToLower().Contains(new_text.ToLower());
		}
	}

	private void _on_cancel_button_pressed()
	{
		Hide();
	}

	private void _on_save_button_pressed()
	{
		// Save
	}

	
	// private void _on_card_name_edit_text_changed(string new_text)
	// {
	// 	CardsListNode.Clear();
		
	// 	// TODO bad?
	// 	foreach (var card in _cards) {
	// 		if (!card.Name.ToLower().Contains(new_text.ToLower()))
	// 			continue;
	// 		var index = CardsListNode.AddItem(card.Name);
	// 		CardsListNode.SetItemMetadata(index, new Wrapper<CardData>(card));
	// 	}
	// }
	
	#endregion
}






