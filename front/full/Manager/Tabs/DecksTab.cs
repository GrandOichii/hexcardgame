using core.decks;
using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class DecksTab : Control
{
	#region Packed scenes
	
	private readonly static PackedScene DeckCardPS = ResourceLoader.Load<PackedScene>("res://Manager/DeckCard.tscn");
	
	#endregion
	
	#region Signals
	
	[Signal]
	public delegate void DecksUpdatedEventHandler(Wrapper<List<DeckTemplate>> decksW);
	
	#endregion
	
	#region Nodes
	
	public ItemList DeckListNode { get; private set; }
	public VBoxContainer DeckCardsNode { get; private set; }
	public HttpRequest GetDecksRequestNode { get; private set; }
	public HttpRequest PostDeckRequestNode { get; private set; }
	public HttpRequest PutDecksRequestNode { get; private set; }
	public Control DeckOverlayNode { get; private set; }
	public Window AddCardWindowNode { get; private set; }
	public LineEdit CardNameEditNode { get; private set; }
	public ItemList CardsListNode { get; private set; }
	public LineEdit NameEditNode { get; private set; }
	public LineEdit NewNameEditNode { get; private set; }
	public Window NewDeckWindowNode { get; private set; }
	public Card NewCardNode { get; private set; }
	public Timer ModifyDecksTimerNode { get; private set; }
	
	#endregion
	
	private string _url;
	private List<core.cards.Card> _cards;
	private List<DeckTemplate> _decks;
	private DeckTemplate _current;

	
	public override void _Ready()
	{
		#region Node fetching
		
		DeckListNode = GetNode<ItemList>("%DeckList");
		DeckCardsNode = GetNode<VBoxContainer>("%DeckCards");
		DeckOverlayNode = GetNode<Control>("%DeckOverlay");
		AddCardWindowNode = GetNode<Window>("%AddCardWindow");
		CardNameEditNode = GetNode<LineEdit>("%CardNameEdit");
		CardsListNode = GetNode<ItemList>("%CardsList");
		NameEditNode = GetNode<LineEdit>("%NameEdit");
		NewNameEditNode = GetNode<LineEdit>("%NewNameEdit");
		NewDeckWindowNode = GetNode<Window>("%NewDeckWindow");
		NewCardNode = GetNode<Card>("%NewCard");
		ModifyDecksTimerNode = GetNode<Timer>("%ModifyDecksTimer");
		
		GetDecksRequestNode = GetNode<HttpRequest>("%GetDecksRequest");
		PostDeckRequestNode = GetNode<HttpRequest>("%PostDeckRequest");
		PutDecksRequestNode = GetNode<HttpRequest>("%PutDecksRequest");
		
		#endregion
	}
	
	public override void _Input(InputEvent e) {
		if (e.IsActionPressed("manager-refresh"))
			Refresh();
	}
	
	public void Refresh() {
		if (_url.Length == 0) {
			GUtil.Alert(this, "Enter backend URL", "Manager");
			return;
		}

		GetDecksRequestNode.Request(_url + "/api/Decks");
	}
	
	public void RequestCreateDeck(DeckTemplate deck) {
		if (NewNameEditNode.Text == "") {
			GUtil.Alert(this, "Enter deck name", "Create deck");
			return;
		}
		var dName = NewNameEditNode.Text;
		foreach (var d in _decks) {
			if (d.GetDescriptor("name") == dName) {
				GUtil.Alert(this, "Deck with name " + dName + " already exists", "Create deck");
				return;
			}
		}
		
		deck.SetDescriptor("name", dName);
		
		// send deck
		var data = deck.ToJson();
		string[] headers = new string[] { "Content-Type: application/json" };
		PostDeckRequestNode.Request(_url + "/api/Decks", headers, HttpClient.Method.Post, data);
		
		NewDeckWindowNode.Hide();
		
		// add deck to list (trust me dude)
		var i = DeckListNode.AddItem(deck.GetDescriptor("name"));
		DeckListNode.SetItemMetadata(i, new Wrapper<DeckTemplate>(deck));
	}

	private void AddCardToList(string cid, int value) {
		var item = DeckCardPS.Instantiate() as DeckCard;
		DeckCardsNode.AddChild(item);
		item.Load(cid, value);
		var c = new Callable(this, "card_value_changed");
		item.Connect("AmountChanged", c);
	}

	private void RecordChangedDeck() {
		if (!ModifyDecksTimerNode.IsStopped()) {
			ModifyDecksTimerNode.Stop();
		}
		ModifyDecksTimerNode.Start();
	}
	
	#region Signal connections

	private void card_value_changed(string cid, int changedTo) {
		_current.Index[cid] = changedTo;
		if (changedTo < 0) _current.Index.Remove(cid);

		RecordChangedDeck();
	}
	
	private void _on_manager_url_updated(string url)
	{
		_url = url;
	}

	private void _on_refresh_button_pressed()
	{
		Refresh();
	}

	private void _on_remove_button_pressed()
	{
		// TODO
	}

	private void _on_request_request_completed(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			GUtil.Alert(this, "Failed to fetch decks data (response code: " + response_code + ")", "Manager");
			return;
		}
		
		DeckOverlayNode.Visible = true;
		var text = System.Text.Encoding.Default.GetString(body);
		_decks = JsonSerializer.Deserialize<List<DeckTemplate>>(text);

		DeckListNode.Clear();
		foreach (var deck in _decks) {
			var i = DeckListNode.AddItem(deck.GetDescriptor("name"));
			DeckListNode.SetItemMetadata(i, new Wrapper<DeckTemplate>(deck));
		}
		
		EmitSignal(SignalName.DecksUpdated, new Wrapper<List<DeckTemplate>>(_decks));
	}

	private void _on_deck_list_item_activated(int index)
	{
		_current = DeckListNode.GetItemMetadata(index).As<Wrapper<DeckTemplate>>().Value;
	
		// fill descriptors
		NameEditNode.Text = _current.GetDescriptor("name");
	
		// fill cards in deck list
		foreach (var child in DeckCardsNode.GetChildren())
			child.Free();
			
		foreach (var pair in _current.Index) {
			AddCardToList(pair.Key, pair.Value);
		}
	
		DeckOverlayNode.Visible = false;
	}

	private void _on_add_card_button_pressed()
	{
		CardNameEditNode.Clear();
		AddCardWindowNode.Show();
	}

	private void _on_generate_button_pressed()
	{
		// TODO
	}

	private void _on_cards_cards_updated(Wrapper<List<core.cards.Card>> cardsW)
	{
		_cards = cardsW.Value;
	}

	private void _on_card_list_item_activated(int index)
	{
		var card = CardsListNode.GetItemMetadata(index).As<Wrapper<core.cards.Card>>().Value;
		
		_current.Index.Add(card.CID, 1);
		AddCardToList(card.CID, 1);
		RecordChangedDeck();

		AddCardWindowNode.Hide();
		NewCardNode.Hide();
	}

	private void _on_card_name_edit_text_changed(string new_text)
	{
		CardsListNode.Clear();
		
		// TODO bad?
		foreach (var card in _cards) {
			if (!card.Name.ToLower().Contains(new_text.ToLower()))
				continue;
			var index = CardsListNode.AddItem(card.Name);
			CardsListNode.SetItemMetadata(index, new Wrapper<core.cards.Card>(card));
		}
	}

	private void _on_add_card_window_close_requested()
	{
		NewCardNode.Hide();
		AddCardWindowNode.Hide();
	}

	private void _on_create_button_pressed()
	{
		NewDeckWindowNode.Show();
		NewNameEditNode.Clear();
	}

	private void _on_new_deck_window_close_requested()
	{
		NewDeckWindowNode.Hide();
	}

	private void _on_empty_button_pressed()
	{
		RequestCreateDeck(new DeckTemplate());
	}

	private void _on_post_deck_request_request_completed(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			GUtil.Alert(this, "Failed to post deck (response code: " + response_code + ")", "Deck creation");
			return;
		}
		
		
	}

	private void _on_card_list_item_selected(int index)
	{
		var card = CardsListNode.GetItemMetadata(index).As<Wrapper<core.cards.Card>>().Value;
		NewCardNode.Show();
		NewCardNode.Load(card);
	}

	private void _on_modify_decks_timer_timeout()
	{
		ModifyDecksTimerNode.Stop();
		var data = JsonSerializer.Serialize(_decks);
		string[] headers = new string[] { "Content-Type: application/json" };
		PutDecksRequestNode.Request(_url + "/api/Decks", headers, HttpClient.Method.Put, data);
	}

	private void _on_put_decks_request_request_completed(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			GUtil.Alert(this, "Failed to put decks (response code: " + response_code + ")", "Deck updating");
			return;
		}
		
		
	}
	
	#endregion
}







