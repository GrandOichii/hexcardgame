using Godot;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Utility;

namespace HexClient.Manager.Tabs;

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
	public ConfirmationDialog DeleteDeckConfirmationPopupNode { get; private set; }
	public DeckEdit DeckEditNode { get; private set; }
	
	public Window DeckEditWindowNode { get; private set; }
	
	public HttpRequest FetchDecksRequestNode { get; private set; }
	public HttpRequest DeleteDeckRequestNode { get; private set; }
	
	#endregion
	
	public override void _Ready()
	{
		#region Node fetching
		
		DeckListNode = GetNode<ItemList>("%DeckList");
		RightNode = GetNode<Control>("%Right");
		DescriptionTextNode = GetNode<TextEdit>("%DescriptionText");
		FetchDecksErrorPopupNode = GetNode<AcceptDialog>("%FetchDecksErrorPopup");
		CardsContainerNode = GetNode<FlowContainer>("%CardsContainer");
		DeleteDeckConfirmationPopupNode = GetNode<ConfirmationDialog>("%DeleteDeckConfirmationPopup");
		DeckEditNode = GetNode<DeckEdit>("%DeckEdit");
		
		DeckEditWindowNode = GetNode<Window>("%DeckEditWindow");
		
		FetchDecksRequestNode = GetNode<HttpRequest>("%FetchDecksRequest");
		DeleteDeckRequestNode = GetNode<HttpRequest>("%DeleteDeckRequest");
		
		#endregion
		
		RightNode.Hide();
	}
	
	private string BaseUrl => GetNode<GlobalSettings>("/root/GlobalSettings").BaseUrl;

	private void UpdateDecks(List<Deck> decks) {
		RightNode.Hide();

		// delete existing
		while (DeckListNode.ItemCount > 0)
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
		var selected = DeckListNode.GetSelectedItems();
		if (selected.Length != 1) {
			// TODO show popup

			return;
		}
		var deckW = DeckListNode.GetItemMetadata(selected[0]).As<Wrapper<Deck>>();

		DeleteDeckConfirmationPopupNode.DialogText = $"Are you sure you want to delete {deckW.Value.Name}?";
		DeleteDeckConfirmationPopupNode.SetMeta("Deck", deckW);
		DeleteDeckConfirmationPopupNode.Show();
	}
	
	private void OnEditButtonPressed()
	{
		var selected = DeckListNode.GetSelectedItems();
		if (selected.Length != 1) {
			// TODO show popup

			return;
		}
		var deck = DeckListNode.GetItemMetadata(selected[0]).As<Wrapper<Deck>>().Value;

		DeckEditNode.Load(deck);
		DeckEditWindowNode.Show();
	}

	private void OnDeleteDeckConfirmationPopupConfirmed()
	{
		// Replace with function body.
		var deck = DeleteDeckConfirmationPopupNode.GetMeta("Deck").As<Wrapper<Deck>>().Value;

		var token = GetNode<GlobalSettings>("/root/GlobalSettings").JwtToken;
		var baseUrl = GetNode<GlobalSettings>("/root/GlobalSettings").BaseUrl;
		
		string[] headers = new string[] { "Content-Type: application/json", $"Authorization: Bearer {token}" };

		DeleteDeckRequestNode.Request(baseUrl + "deck/" + Uri.EscapeDataString(deck.Id), headers, HttpClient.Method.Delete);

	}

	private void OnDeleteDeckRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			// TODO check response code
			GD.Print("failed to delete deck " + response_code);
			return;
		}

		// TODO if successfully deleted, show user another popup
		RightNode.Hide();
		OnFetchDecksButtonPressed();
	}

	private void OnDeckEditWindowCloseRequested()
	{
		DeckEditNode.TryClose();
	}
	private void OnDeckEditClosed()
	{
		DeckEditWindowNode.Hide();
	}
	
	#endregion
}
