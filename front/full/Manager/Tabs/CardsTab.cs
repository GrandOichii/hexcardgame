using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class CardsTab : Control
{
	#region Signals
	
	[Signal]
	public delegate void CardsUpdatedEventHandler(Wrapper<List<CardData>> cardsW);
	
	#endregion
	
	#region Packed scenes

	private readonly static PackedScene CardsTabCardPS = ResourceLoader.Load<PackedScene>("res://Manager/CardsTabCard.tscn");

	#endregion

	#region Node fetching
	
	public HttpRequest CardsRequestNode { get; private set; }
	public HttpRequest ExpansionsRequestNode { get; private set; }
	public HttpRequest PutCardRequestNode { get; private set; }
	public FlowContainer CardsContainerNode { get; private set; }
	public ItemList ExpansionListNode { get; private set; }
	public LineEdit NameFilterEditNode { get; private set; }
	public CardEditWindow CardEditWindowNode { get; private set; }
	
	#endregion
	
	private String _url = "";
	private List<CardData> _cards;
	
	public override void _Ready()
	{
		#region Node fetching
		
		CardsRequestNode = GetNode<HttpRequest>("%CardsRequest");
		ExpansionsRequestNode = GetNode<HttpRequest>("%ExpansionsRequest");
		PutCardRequestNode = GetNode<HttpRequest>("%PutCardRequest");
		CardsContainerNode = GetNode<FlowContainer>("%CardsContainer");
		ExpansionListNode = GetNode<ItemList>("%ExpansionList");
		NameFilterEditNode = GetNode<LineEdit>("%NameFilterEdit");
		CardEditWindowNode = GetNode<CardEditWindow>("%CardEditWindow");
		
		#endregion
	}
	
	public void Refresh() {
		if (_url.Length == 0) {
			GUtil.Alert(this, "Enter backend URL", "Manager");
			return;
		}

		CardsRequestNode.Request(_url + "/api/Cards");
		ExpansionsRequestNode.Request(_url + "/api/Expansions");
	}

	public override void _Input(InputEvent e)
	{
		if (e.IsActionPressed("manager-refresh"))
			Refresh();
	}

	#region Signal connections

	private void _on_refresh_button_pressed()
	{
		Refresh();
	}

	private void _on_request_request_completed(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			GUtil.Alert(this, "Failed to fetch cards data (response code: " + response_code + ")", "Manager");
			return;
		}
		
		var text = System.Text.Encoding.Default.GetString(body);
		_cards = JsonSerializer.Deserialize<List<CardData>>(text);

		var cCount = CardsContainerNode.GetChildCount();
		var nCount = _cards.Count;

		if (nCount > cCount) {
			// fill hand up to new count
			for (int i = 0; i < nCount - cCount; i++) {
				var child = CardsTabCardPS.Instantiate() as CardsTabCard;
				CardsContainerNode.AddChild(child);
				var c = new Callable(this, "_card_edit_requested");
				child.Connect("CardEditRequested", c);
			}
		}
		if (nCount < cCount) {
			// trim child count
			for (int i = cCount - 1; i >= nCount; i--) {
				var child = CardsContainerNode.GetChild(i);
				child.Free();
			}
		}

		ExpansionListNode.Clear();
		ExpansionListNode.AddItem("All");

		// load card data
		for (int i = 0; i < nCount; i++) {
			var card = CardsContainerNode.GetChild(i) as CardsTabCard;
			var c = _cards[i];
			card.Load(c);
		}
		
		EmitSignal(SignalName.CardsUpdated, new Wrapper<List<CardData>>(_cards));

	}

	private void _on_manager_url_updated(string url)
	{
		_url = url;
	}

	private void _on_name_filter_edit_text_changed(string new_text)
	{
		foreach (var child in CardsContainerNode.GetChildren()) {
			var card = child as CardsTabCard;
			card.Visible = card.CardNode.CardState.Name.ToLower().Contains(new_text.ToLower());
		}
	}

	private void _on_expansion_list_item_activated(int index)
	{
		// TODO replace
		// var eName = ExpansionListNode.GetItemText(index);
		// foreach (var child in CardsContainerNode.GetChildren()) {
		// 	var card = child as CardsTabCard;
		// 	card.Visible = eName == "All" || card.CardNode.CardState.Expansion == eName;
		// }

	}

	private void _on_expansions_request_request_completed(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			GUtil.Alert(this, "Failed to fetch expansions data (response code: " + response_code + ")", "Manager");
			return;
		}

		var text = System.Text.Encoding.Default.GetString(body);
		var expansions = JsonSerializer.Deserialize<List<ExpansionData>>(text);
		foreach (var e in expansions) {
			var ei = ExpansionListNode.AddItem(e.Name);

		}
	}

	private void _card_edit_requested(Wrapper<CardData> cardW) {
		var card = cardW.Value;
		CardEditWindowNode.Edit(card);
	}

	private void _on_card_edit_window_card_edited(string oldName, Wrapper<CardData> cardW)
	{
		// Replace with function body.
		var card = cardW.Value;
		var data = card.ToJson();
		string[] headers = new string[] { "Content-Type: application/json" };
		var url = _url + "/api/Cards?oldName=" + oldName.URIEncode();
		var method = HttpClient.Method.Put;
		if (oldName == "") {
			url = _url + "/api/Cards";
			method = HttpClient.Method.Post;
		}
		PutCardRequestNode.Request(url, headers, method, data);
	}

	private void _on_put_card_request_request_completed(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			GUtil.Alert(this, "Failed to edit card data (response code: " + response_code + ")", "Manager");
			return;
		}
		
		Refresh();
		NameFilterEditNode.Clear();
	}

	private void _on_add_button_pressed()
	{
		CardEditWindowNode.Edit();
	}

	#endregion
}



