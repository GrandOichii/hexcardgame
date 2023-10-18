using core.cards;
using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class CardsTab : Control
{
	#region Signals
	
	[Signal]
	public delegate void CardsUpdatedEventHandler(Wrapper<List<ExpansionCard>> cardsW);
	
	#endregion
	
	#region Packed scenes

	private readonly static PackedScene CardPS = ResourceLoader.Load<PackedScene>("res://Match/Cards/Card.tscn");

	#endregion

	#region Node fetching
	
	public HttpRequest CardsRequestNode { get; private set; }
	public HttpRequest ExpansionsRequestNode { get; private set; }
	public FlowContainer CardsContainerNode { get; private set; }
	public ItemList ExpansionListNode { get; private set; }
	public LineEdit NameFilterEditNode { get; private set; }
	
	#endregion
	
	private String _url = "";
	private List<ExpansionCard> _cards;
	
	public override void _Ready()
	{
		#region Node fetching
		
		CardsRequestNode = GetNode<HttpRequest>("%CardsRequest");
		ExpansionsRequestNode = GetNode<HttpRequest>("%ExpansionsRequest");
		CardsContainerNode = GetNode<FlowContainer>("%CardsContainer");
		ExpansionListNode = GetNode<ItemList>("%ExpansionList");
		NameFilterEditNode = GetNode<LineEdit>("%NameFilterEdit");
		
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
		_cards = JsonSerializer.Deserialize<List<ExpansionCard>>(text);

		var cCount = CardsContainerNode.GetChildCount();
		var nCount = _cards.Count;

		if (nCount > cCount) {
			// fill hand up to new count
			for (int i = 0; i < nCount - cCount; i++) {
				var child = CardPS.Instantiate() as Card;
				CardsContainerNode.AddChild(child);
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
			var card = CardsContainerNode.GetChild(i) as Card;
			var c = _cards[i];
			card.Load(c);
		}
		
		EmitSignal(SignalName.CardsUpdated, new Wrapper<List<ExpansionCard>>(_cards));

	}

	private void _on_manager_url_updated(string url)
	{
		_url = url;
	}

	private void _on_name_filter_edit_text_changed(string new_text)
	{
		foreach (var child in CardsContainerNode.GetChildren()) {
			var card = child as Card;
			card.Visible = card.CardState.Name.ToLower().Contains(new_text.ToLower());
		}
	}

	private void _on_expansion_list_item_activated(int index)
	{
		var eName = ExpansionListNode.GetItemText(index);
		foreach (var child in CardsContainerNode.GetChildren()) {
			var card = child as Card;
			card.Visible = eName == "All" || card.CardState.Expansion == eName;
		}

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

	#endregion
}
