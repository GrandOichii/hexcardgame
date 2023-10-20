using core.cards;
using core.decks;
using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class DecksTab : Control
{
	#region Signals
	
	[Signal]
	public delegate void DecksUpdatedEventHandler(Wrapper<List<DeckData>> decksW);
	
	#endregion
	
	#region Nodes
	
	public ItemList DeckListNode { get; private set; }
	public HttpRequest GetDecksRequestNode { get; private set; }
	public HttpRequest PostDeckRequestNode { get; private set; }
	public HttpRequest PutDecksRequestNode { get; private set; }
	public Control DeckOverlayNode { get; private set; }
	public Window NewDeckWindowNode { get; private set; }
	public Timer ModifyDecksTimerNode { get; private set; }
	public ItemList DeckCardsNode { get; private set; }
	public Card CardNode { get; private set; }
	public DeckEditWindow DeckEditWindowNode { get; private set; }
	public LineEdit NewNameEditNode { get; private set; }

	#endregion
	
	private string _url;
	private List<CardData> _cards;
	private List<DeckData> _decks;
	private DeckData _current;

	
	public override void _Ready()
	{
		#region Node fetching
		
		DeckListNode = GetNode<ItemList>("%DeckList");
		DeckOverlayNode = GetNode<Control>("%DeckOverlay");
		DeckCardsNode = GetNode<ItemList>("%DeckCards");
		CardNode = GetNode<Card>("%Card");
		DeckEditWindowNode = GetNode<DeckEditWindow>("%DeckEditWindow");
		NewNameEditNode = GetNode<LineEdit>("%NewNameEdit");
		
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
	
	public void RequestCreateDeck(DeckData deck) {
		if (NewNameEditNode.Text == "") {
			GUtil.Alert(this, "Enter deck name", "Create deck");
			return;
		}
		var dName = NewNameEditNode.Text;
		foreach (var d in _decks) {
			if (d.Name == dName) {
				GUtil.Alert(this, "Deck with name " + dName + " already exists", "Create deck");
				return;
			}
		}
		
		deck.Name = dName;
		
		// send deck
		// TODO return?
		// var data = deck.ToJson();
		// string[] headers = new string[] { "Content-Type: application/json" };
		// PostDeckRequestNode.Request(_url + "/api/Decks", headers, HttpClient.Method.Post, data);
		
		NewDeckWindowNode.Hide();
		
		// add deck to list (trust me dude)
		var i = DeckListNode.AddItem(deck.Name);
		DeckListNode.SetItemMetadata(i, new Wrapper<DeckData>(deck));
	}

	private void RecordChangedDeck() {
		if (!ModifyDecksTimerNode.IsStopped()) {
			ModifyDecksTimerNode.Stop();
		}
		ModifyDecksTimerNode.Start();
	}
	
	#region Signal connections
	
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
		
		// DeckOverlayNode.Visible = true;
		var text = System.Text.Encoding.Default.GetString(body);
		_decks = JsonSerializer.Deserialize<List<DeckData>>(text);

		DeckListNode.Clear();
		foreach (var deck in _decks) {
			var i = DeckListNode.AddItem(deck.Name);
			DeckListNode.SetItemMetadata(i, new Wrapper<DeckData>(deck));
		}
		
		EmitSignal(SignalName.DecksUpdated, new Wrapper<List<DeckData>>(_decks));
	}

	private void _on_generate_button_pressed()
	{
		// TODO
	}

	private void _on_cards_cards_updated(Wrapper<List<CardData>> cardsW)
	{
		_cards = cardsW.Value;
	}


	private void _on_create_button_pressed()
	{
		NewDeckWindowNode.Show();
	}

	private void _on_new_deck_window_close_requested()
	{
		NewDeckWindowNode.Hide();
	}

	private void _on_empty_button_pressed()
	{
		RequestCreateDeck(new DeckData());
	}

	private void _on_post_deck_request_request_completed(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			GUtil.Alert(this, "Failed to post deck (response code: " + response_code + ")", "Deck creation");
			return;
		}
	}

	private void _on_put_decks_request_request_completed(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			GUtil.Alert(this, "Failed to put decks (response code: " + response_code + ")", "Deck updating");
			return;
		}
		
		
	}

	private void _on_deck_list_item_activated(int index)
	{
		_current = DeckListNode.GetItemMetadata(index).As<Wrapper<DeckData>>().Value;
		
		DeckCardsNode.Clear();
		foreach (var card in _current.Cards) {
			var i = DeckCardsNode.AddItem(card.CID + " x" + card.Amount);
			DeckCardsNode.SetItemMetadata(i, new Wrapper<DeckCardData>(card));
		}
	}

	private void _on_deck_cards_item_selected(int index)
	{
		var card = DeckCardsNode.GetItemMetadata(index).As<Wrapper<DeckCardData>>().Value;
		CardNode.Load(card.Card.Card);
	}

	private void _on_edit_button_pressed()
	{
		DeckEditWindowNode.Load(_current);
	}
	
	#endregion
}
