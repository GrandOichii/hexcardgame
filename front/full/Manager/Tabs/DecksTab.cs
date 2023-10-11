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
	public HttpRequest RequestNode { get; private set; }
	public Control DeckOverlayNode { get; private set; }
	
	#endregion
	
	private string _url;
	
	public override void _Ready()
	{
		#region Node fetching
		
		DeckListNode = GetNode<ItemList>("%DeckList");
		DeckCardsNode = GetNode<VBoxContainer>("%DeckCards");
		RequestNode = GetNode<HttpRequest>("%Request");
		DeckOverlayNode = GetNode<Control>("%DeckOverlay");
		
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

		RequestNode.Request(_url + "/api/Decks");
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
		
		DeckOverlayNode.Visible = true;
		var text = System.Text.Encoding.Default.GetString(body);
		var data = JsonSerializer.Deserialize<List<DeckTemplate>>(text);

		DeckListNode.Clear();
		foreach (var deck in data) {
			var i = DeckListNode.AddItem(deck.GetDescriptor("name"));
			DeckListNode.SetItemMetadata(i, new Wrapper<DeckTemplate>(deck));
		}
		
		EmitSignal(SignalName.DecksUpdated, new Wrapper<List<DeckTemplate>>(data));
	}

	private void _on_deck_list_item_activated(int index)
	{
		var deck = DeckListNode.GetItemMetadata(index).As<Wrapper<DeckTemplate>>().Value;
	
		foreach (var child in DeckCardsNode.GetChildren())
			child.Free();
			
		foreach (var pair in deck.Index) {
			var item = DeckCardPS.Instantiate<DeckCard>();
			DeckCardsNode.AddChild(item);
			item.Load(pair.Key, pair.Value);
		}
	
		DeckOverlayNode.Visible = false;
	}

	private void _on_add_card_button_pressed()
	{
		// TODO
	}

	private void _on_import_button_pressed()
	{
		// TODO
	}

	private void _on_generate_button_pressed()
	{
		// TODO
	}
	
	#endregion
}
