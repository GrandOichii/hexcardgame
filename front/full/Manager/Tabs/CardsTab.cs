using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class CardsTab : Control
{
	#region Packed scenes

	private readonly static PackedScene CardPS = ResourceLoader.Load<PackedScene>("res://Match/Cards/Card.tscn");

	#endregion

	#region Node fetching
	
	public HttpRequest RequestNode { get; private set; }
	public FlowContainer CardsContainerNode { get; private set; }
	public ItemList ExpansionListNode { get; private set; }
	public LineEdit NameFilterEditNode { get; private set; }
	
	#endregion
	
	private String _url = "";
	private List<core.cards.Card> _cards;
	
	public override void _Ready()
	{
		#region Node fetching
		
		RequestNode = GetNode<HttpRequest>("%Request");
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

		RequestNode.Request(_url + "/api/Cards");
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
		_cards = JsonSerializer.Deserialize<List<core.cards.Card>>(text);

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

		HashSet<string> expansions = new();
		// load card data
		for (int i = 0; i < nCount; i++) {
			var card = CardsContainerNode.GetChild(i) as Card;
			var c = _cards[i];
			card.Load(c);

			if (expansions.Contains(c.Expansion)) continue;

			expansions.Add(c.Expansion);
			var ei = ExpansionListNode.AddItem(c.Expansion);
		}

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

		// NameFilterEditNode.Text = "";
	}

	#endregion
}







