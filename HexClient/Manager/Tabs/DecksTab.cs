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
	public DeckEdit DeckEditNode { get; private set; }
	
	public AcceptDialog FetchDecksErrorPopupNode { get; private set; }
	public FlowContainer CardsContainerNode { get; private set; }
	public ConfirmationDialog DeleteDeckConfirmationPopupNode { get; private set; }
	public Window DeckEditWindowNode { get; private set; }
	public AcceptDialog DeleteDeckErrorPopupNode { get; private set; }
	public AcceptDialog DeletedPopupNode { get; private set; }
	public AcceptDialog UpdateDeckErrorPopupNode { get; private set; }
	
	public HttpRequest FetchDecksRequestNode { get; private set; }
	public HttpRequest DeleteDeckRequestNode { get; private set; }
	public HttpRequest CreateDeckRequestNode { get; private set; }
	public HttpRequest UpdateCardRequestNode { get; private set; }
	
	#endregion
	
	private Deck? _current = null;

	public override void _Ready()
	{
		#region Node fetching
		
		DeckListNode = GetNode<ItemList>("%DeckList");
		RightNode = GetNode<Control>("%Right");
		DescriptionTextNode = GetNode<TextEdit>("%DescriptionText");
		CardsContainerNode = GetNode<FlowContainer>("%CardsContainer");
		DeckEditNode = GetNode<DeckEdit>("%DeckEdit");
		
		FetchDecksErrorPopupNode = GetNode<AcceptDialog>("%FetchDecksErrorPopup");
		DeleteDeckConfirmationPopupNode = GetNode<ConfirmationDialog>("%DeleteDeckConfirmationPopup");
		DeckEditWindowNode = GetNode<Window>("%DeckEditWindow");
		DeleteDeckErrorPopupNode = GetNode<AcceptDialog>("%DeleteDeckErrorPopup");
		DeletedPopupNode = GetNode<AcceptDialog>("%DeletedPopup");
		UpdateDeckErrorPopupNode = GetNode<AcceptDialog>("%UpdateDeckErrorPopup");
		
		FetchDecksRequestNode = GetNode<HttpRequest>("%FetchDecksRequest");
		DeleteDeckRequestNode = GetNode<HttpRequest>("%DeleteDeckRequest");
		CreateDeckRequestNode = GetNode<HttpRequest>("%CreateDeckRequest");
		UpdateCardRequestNode = GetNode<HttpRequest>("%UpdateCardRequest");
		
		#endregion
		
		RightNode.Hide();
	}
	
	private string BaseUrl => GetNode<GlobalSettings>("/root/GlobalSettings").ApiUrl;

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
		DeckEditNode.Load(null);
		DeckEditWindowNode.Show();
	}

	private void OnDeckListItemActivated(int index)
	{
		_current = DeckListNode.GetItemMetadata(index).As<Wrapper<Deck>>().Value;
		DescriptionTextNode.Text = _current.Value.Description;

		while (CardsContainerNode.GetChildCount() > 0)
			CardsContainerNode.RemoveChild(CardsContainerNode.GetChild(0));

		foreach (var pair in _current.Value.Index) {
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
			DeleteDeckErrorPopupNode.DialogText = "Select a deck to delete";
			DeleteDeckErrorPopupNode.Show();

			return;
		}
		var deckW = DeckListNode.GetItemMetadata(selected[0]).As<Wrapper<Deck>>();

		DeleteDeckConfirmationPopupNode.DialogText = $"Are you sure you want to delete {deckW.Value.Name}?";
		DeleteDeckConfirmationPopupNode.SetMeta("Deck", deckW);
		DeleteDeckConfirmationPopupNode.Show();
	}
	
	private void OnEditButtonPressed()
	{
		if (_current is null) {
			// * shouldn't even happen
			return;
		}

		DeckEditNode.Load(_current);
		DeckEditWindowNode.Show();
	}

	private void OnDeleteDeckConfirmationPopupConfirmed()
	{
		// Replace with function body.
		var deck = DeleteDeckConfirmationPopupNode.GetMeta("Deck").As<Wrapper<Deck>>().Value;

		var token = GetNode<GlobalSettings>("/root/GlobalSettings").JwtToken;
		var baseUrl = GetNode<GlobalSettings>("/root/GlobalSettings").ApiUrl;
		
		string[] headers = new string[] { "Content-Type: application/json", $"Authorization: Bearer {token}" };

		DeleteDeckRequestNode.Request(baseUrl + "deck/" + Uri.EscapeDataString(deck.Id), headers, HttpClient.Method.Delete);

	}

	private void OnDeleteDeckRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			var resp = Encoding.UTF8.GetString(body);
			DeleteDeckErrorPopupNode.DialogText = $"Failed to delete deck! (code: {response_code})\n\n{resp}";
			DeleteDeckErrorPopupNode.Show();
			return;
		}

		var deckName = DeleteDeckConfirmationPopupNode.GetMeta("Deck").As<Wrapper<Deck>>().Value.Name;
		DeleteDeckConfirmationPopupNode.RemoveMeta("Deck");
		DeletedPopupNode.DialogText = $"Succesfully deleted deck {deckName}";
		DeletedPopupNode.Show();

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
	
	private void OnDeckEditSaved(Wrapper<Deck> deckW, string oldId)
	{
		var token = GetNode<GlobalSettings>("/root/GlobalSettings").JwtToken;
		var baseUrl = GetNode<GlobalSettings>("/root/GlobalSettings").ApiUrl;
		
		string[] headers = new string[] { "Content-Type: application/json", $"Authorization: Bearer {token}" };
		var deck = deckW.Value;

		if (!string.IsNullOrEmpty(oldId)) {

			GD.Print(baseUrl + "deck/" + Uri.EscapeDataString(deck.Id));
			UpdateCardRequestNode.Request(baseUrl + "deck/" + Uri.EscapeDataString(deck.Id), headers, HttpClient.Method.Put, JsonSerializer.Serialize(deck, Common.JSON_SERIALIZATION_OPTIONS));
			return;
		}

		CreateDeckRequestNode.Request(baseUrl + "deck", headers, HttpClient.Method.Post, JsonSerializer.Serialize(deck, Common.JSON_SERIALIZATION_OPTIONS));
	}
	
	private void OnCreateDeckRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {

			return;
		}

		DeckEditWindowNode.Hide();

		RightNode.Hide();
		OnFetchDecksButtonPressed();
	}

	private void OnUpdateCardRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			var resp = Encoding.UTF8.GetString(body);
			UpdateDeckErrorPopupNode.DialogText = $"Failed to delete deck! (code: {response_code})\n\n{resp}";
			UpdateDeckErrorPopupNode.Show();

			return;
		}

		DeckEditWindowNode.Hide();

		RightNode.Hide();
		OnFetchDecksButtonPressed();

		// TODO annoying, after saving the edits closes the edit window and doesn't show the updates in the main window

	}

	#endregion
}
