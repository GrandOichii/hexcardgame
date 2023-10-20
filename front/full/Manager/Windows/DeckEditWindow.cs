using Godot;
using System;
using System.Collections.Generic;

public partial class DeckEditWindow : Window
{
	#region Packed scenes
	
	private readonly static PackedScene DeckCardPS = ResourceLoader.Load<PackedScene>("res://Manager/DeckCard.tscn");
	
	#endregion

	#region Signals

	[Signal]
	public delegate void DeckEditedEventHandler(string oldName, Wrapper<DeckData> deckW);

	#endregion

	#region Nodes
	
	public Window AddCardWindowNode { get; private set; }
	public LineEdit NameEditNode { get; private set; }
	public TextEdit DescriptionEditNode { get; private set; }
	public VBoxContainer CardsContainerNode { get; private set; }
	public ItemList CardsListNode { get; private set; }
	public LineEdit NewCardNameFilterNode { get; private set; }

	#endregion
	
	private DeckData _current;
	public List<CardData> Cards { get; set; }

	public override void _Ready()
	{
		#region Node fethching
		
		NameEditNode = GetNode<LineEdit>("%NameEdit");
		DescriptionEditNode = GetNode<TextEdit>("%DescriptionEdit");
		CardsContainerNode = GetNode<VBoxContainer>("%CardsContainer");
		AddCardWindowNode = GetNode<Window>("%AddCardWindow");
		CardsListNode = GetNode<ItemList>("%CardsList");
		NewCardNameFilterNode = GetNode<LineEdit>("%NewCardNameFilter");
		
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
			AddCardToDeck(card);
		}

		Show();
	}

	private void AddCardToDeck(DeckCardData card) {
		var child = DeckCardPS.Instantiate() as DeckCard;
		CardsContainerNode.AddChild(child);

		child.Load(card);
	}

	private DeckData Baked {
		get {
			var result = new DeckData();

			// TODO
			result.Name = NameEditNode.Text;

			// TODO add back
			// result.Description = DescriptionEditNode.Text;
			
			result.Cards = new();
			foreach (var c in CardsContainerNode.GetChildren()) {
				var child = c as DeckCard;
				var b = child.Baked;
				b.DeckNameKey = result.Name;
				result.Cards.Add(b);
			}

			return result;
		}
	}
	
	#region Signal connections
	
	private void _on_close_requested()
	{
		Hide();
	}

	private void _on_add_button_pressed()
	{
		NewCardNameFilterNode.Clear();
		AddCardWindowNode.Show();
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
		var result = Baked;
		string oldName = "";
		if (_current is not null) oldName = _current.Name;
		Hide();
		EmitSignal(SignalName.DeckEdited, oldName, new Wrapper<DeckData>(Baked));
	}

	private void _on_new_card_name_filter_text_changed(string new_text)
	{
		CardsListNode.Clear();
		
		// TODO bad?
		foreach (var card in Cards) {
			if (!card.Name.ToLower().Contains(new_text.ToLower()))
				continue;
			var index = CardsListNode.AddItem(card.Name);
			CardsListNode.SetItemMetadata(index, new Wrapper<CardData>(card));
		}
	}

	private void _on_add_card_window_close_requested()
	{
		AddCardWindowNode.Hide();
	}

	private void _on_cards_list_item_activated(int index)
	{
		var card = CardsListNode.GetItemMetadata(index).As<Wrapper<CardData>>().Value;
		
		foreach (var c in CardsContainerNode.GetChildren()) {
			var child = c as DeckCard;
			var dCard = child.Data;
			if (dCard.Card.Card.Name == card.Name) {
				++child.Amount;
				
				AddCardWindowNode.Hide();
				
				return;
			}
		}

		var newCard = new DeckCardData();
		newCard.Amount = 1;
		newCard.Card = new();
		newCard.Card.Expansion = card.Expansions[0].Name;
		newCard.Card.Card = card;

		AddCardToDeck(newCard);
		
		AddCardWindowNode.Hide();
	}
	
	#endregion
}



