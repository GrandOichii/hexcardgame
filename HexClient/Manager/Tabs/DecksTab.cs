using Godot;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Utility;

namespace HexClient.Manager.Tabs;

public struct Deck {
	public required string Id { get; set; }
	public required string Name { get; set; }
	public required string Description { get; set; }
	public required Dictionary<string, int> Index { get; set; }
}

public interface IDeckCardDisplay {
	public void Load(string cid, int amount);
}

public partial class DecksTab : Control
{
	#region Packed scenes

	[Export]
	private PackedScene DeckCardDisplayPS { get; set; }

	#endregion

	#region Nodes
	
	public ItemList DeckListNode { get; private set; }
	public Control RightNode { get; private set; } 
	public TextEdit DescriptionTextNode { get; private set; }
	public AcceptDialog FetchDecksErrorPopupNode { get; private set; }
	public FlowContainer CardsContainerNode { get; private set; }
	
	public HttpRequest FetchDecksRequestNode { get; private set; }
	
	#endregion
	
	public override void _Ready()
	{
		#region Node fetching
		
		DeckListNode = GetNode<ItemList>("%DeckList");
		RightNode = GetNode<Control>("%Right");
		DescriptionTextNode = GetNode<TextEdit>("%DescriptionText");
		FetchDecksErrorPopupNode = GetNode<AcceptDialog>("%FetchDecksErrorPopup");
		CardsContainerNode = GetNode<FlowContainer>("%CardsContainer");
		
		FetchDecksRequestNode = GetNode<HttpRequest>("%FetchDecksRequest");
		
		#endregion
		
		RightNode.Hide();
		//OnFetchDecksButtonPressed();
	}
	
	private string BaseUrl => GetNode<GlobalSettings>("/root/GlobalSettings").BaseUrl;

	private void UpdateDecks(List<Deck> decks) {
		RightNode.Hide();

		// delete existing
		while (DeckListNode.GetChildCount() > 0)
			DeckListNode.RemoveItem(0);

		// set data
		foreach (var deck in decks) {
			var i = DeckListNode.AddItem(deck.Name);
			DeckListNode.SetItemMetadata(i, new Wrapper<Deck>(deck));
		}
	}

	#region Signal connections
	
	private void OnCreateButtonPressed()
	{
		// TODO
		
	}

	private void OnButtonPressed()
	{
		// TODO
		
	}

	private void OnDeckListItemActivated(int index)
	{
		var deck = DeckListNode.GetItemMetadata(index).As<Wrapper<Deck>>().Value;
		DescriptionTextNode.Text = deck.Description;

		foreach (var pair in deck.Index) {
			var cid = pair.Key;
			var amount = pair.Value;

			var child = DeckCardDisplayPS.Instantiate();
			CardsContainerNode.AddChild(child);

			var display = child as IDeckCardDisplay;
			display.Load(cid, amount);
		}
		
		RightNode.Show();
	}

	private void OnFetchDecksRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			var resp = Encoding.UTF8.GetString(body);
			FetchDecksErrorPopupNode.DialogText = $"Failed to fetch decks! (code: {response_code})\n\n{resp}";
			FetchDecksErrorPopupNode.Show();
			
			return; 
		}
		
		var decks = JsonSerializer.Deserialize<List<Deck>>(body, Common.JSON_SERIALIZATION_OPTIONS);
		UpdateDecks(decks);
	}
	
	private void OnFetchDecksButtonPressed()
	{
		var token = GetNode<GlobalSettings>("/root/GlobalSettings").JwtToken;
		string[] headers = new string[] { "Content-Type: application/json", $"Authorization: Bearer {token}" };

		FetchDecksRequestNode.Request(BaseUrl + "deck", headers);
	}

	private void OnDeleteButtonPressed()
	{
		// TODO
	}
	
	private void OnEditButtonPressed()
	{
		// TODO
	}
	
	#endregion
}


