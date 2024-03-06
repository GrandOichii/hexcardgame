using Godot;
using HexCore.Cards;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Utility;

namespace HexClient.Manager.Tabs;

public struct Expansion {
	public string Name { get; set; }
	public int CardCount { get; set; }
}

public interface ICardDisplay {
	public void Load(ExpansionCard card);
	public void SubscribeToRightClick(Action<Wrapper<ExpansionCard>> a);
}

public partial class CardsTab : Control
{
	#region Packed scenes

	[Export]
	private PackedScene CardDisplayPS { get; set; }

	#endregion

	#region Nodes
	
	public ItemList ExpansionsListNode { get; private set; }
	public FlowContainer CardsContainerNode { get; private set; }

	public Window CardEditWindowNode { get; private set; }
	public CardEdit CardEditNode { get; private set; }
	public PopupMenu CardContextMenuNode { get; private set; }

	public AcceptDialog CardAlertPopupNode { get; private set; }


	public HttpRequest FetchExpansionsRequestNode { get; private set; }
	public HttpRequest FetchExpansionCardsRequestNode { get; private set; }
	public HttpRequest CreateCardRequestNode { get; private set; }
	public HttpRequest DeleteCardRequestNode { get; private set; }
	public HttpRequest UpdateCardRequestNode { get; private set; }
	
	#endregion
	
	public override void _Ready()
	{
		#region Node fetching
		
		ExpansionsListNode = GetNode<ItemList>("%ExpansionsList");
		CardsContainerNode = GetNode<FlowContainer>("%CardsContainer");

		CardContextMenuNode = GetNode<PopupMenu>("%CardContextMenu");

		CardEditWindowNode = GetNode<Window>("%CardEditWindow");
		CardEditNode = GetNode<CardEdit>("%CardEdit");
		
		CardAlertPopupNode = GetNode<AcceptDialog>("%CardAlertPopup");

		FetchExpansionsRequestNode = GetNode<HttpRequest>("%FetchExpansionsRequest");
		FetchExpansionCardsRequestNode = GetNode<HttpRequest>("%FetchExpansionCardsRequest");
		CreateCardRequestNode = GetNode<HttpRequest>("%CreateCardRequest");
		DeleteCardRequestNode = GetNode<HttpRequest>("%DeleteCardRequest");
		UpdateCardRequestNode = GetNode<HttpRequest>("%UpdateCardRequest");
		
		#endregion

		CardEditWindowNode.Hide();
		OnFetchExpansionsButtonPressed();
	}

	private void UpdateExpansion(Expansion expansion) {
		// TODO check existing

		var itemI = ExpansionsListNode.AddItem($"{expansion.Name} ({expansion.CardCount})");
		ExpansionsListNode.SetItemMetadata(itemI, new Wrapper<Expansion>(expansion));
	}

	private void LoadExpansion(string expansion) {
		while (CardsContainerNode.GetChildCount() > 0)
			CardsContainerNode.RemoveChild(CardsContainerNode.GetChild(0));

		var baseUrl = GetNode<GlobalSettings>("/root/GlobalSettings").BaseUrl;
		FetchExpansionCardsRequestNode.Request(baseUrl + "card/fromexpansion/" + expansion);
	}

	#region Signal connections
	
	private void OnExpansionsListItemActivated(int index)
	{
		var expansion = ExpansionsListNode.GetItemMetadata(index).As<Wrapper<Expansion>>().Value;

		LoadExpansion(expansion.Name);
	}

	private void OnFetchExpansionsButtonPressed()
	{
		var baseUrl = GetNode<GlobalSettings>("/root/GlobalSettings").BaseUrl;
		FetchExpansionsRequestNode.Request(baseUrl + "expansion");
	}

	private void OnFetchExpansionsRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		// TODO check response code

		var expansions = JsonSerializer.Deserialize<List<Expansion>>(body, Common.JSON_SERIALIZATION_OPTIONS);
		foreach (var expansion in expansions) {
			UpdateExpansion(expansion);
		}
	}
	
	private void OnFetchExpansionCardsRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		// TODO check response code
		
		var cards = JsonSerializer.Deserialize<List<ExpansionCard>>(body, Common.JSON_SERIALIZATION_OPTIONS);
		foreach (var card in cards) {
			var child = CardDisplayPS.Instantiate();
			CardsContainerNode.AddChild(child);

			var cardDisplay = child as ICardDisplay;
			cardDisplay.Load(card);
			cardDisplay.SubscribeToRightClick(OnCardRightClick);
		}
	}

	private void OnCardRightClick(Wrapper<ExpansionCard> cardW) {
		if (!GetNode<GlobalSettings>("/root/GlobalSettings").IsAdmin)
			return;

		var mousePos = GetGlobalMousePosition();
		CardContextMenuNode.PopupOnParent(new Rect2I((int)mousePos.X, (int)mousePos.Y, -1, -1));
		CardContextMenuNode.Show();
		CardContextMenuNode.SetMeta("Card", cardW);
	}

	private void OnCreateCardButtonPressed()
	{
		CardEditNode.Load(null);
		CardEditWindowNode.Show();
	}

	private void OnCardEditWindowCloseRequested()
	{
		CardEditWindowNode.Hide();
	}

	private void OnCardEditClosed()
	{
		CardEditWindowNode.Hide();
	}
	
	private void OnCardEditSaved(Wrapper<ExpansionCard> cardW, string oldName)
	{
		var token = GetNode<GlobalSettings>("/root/GlobalSettings").JwtToken;
		var baseUrl = GetNode<GlobalSettings>("/root/GlobalSettings").BaseUrl;
		
		string[] headers = new string[] { "Content-Type: application/json", $"Authorization: Bearer {token}" };
		var card = cardW.Value;

		if (string.IsNullOrEmpty(oldName)) {
			// create card
			// TODO validate that the name is not taken
			
			CreateCardRequestNode.Request(baseUrl + "card", headers, HttpClient.Method.Post, JsonSerializer.Serialize(card, Common.JSON_SERIALIZATION_OPTIONS));
			return;
		}

		// update card
		// TODO check if cid is same
		UpdateCardRequestNode.Request(baseUrl + "card", headers, HttpClient.Method.Put, JsonSerializer.Serialize(card, Common.JSON_SERIALIZATION_OPTIONS));
	}
	
	private void OnCreateCardRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			var resp = Encoding.UTF8.GetString(body);

			CardAlertPopupNode.DialogText = $"Failed to create card! (Response code: {response_code}\nResponse message:\n{resp}";
			CardAlertPopupNode.Show();
			return;
		}

		CardEditWindowNode.Hide();
		
		// * this parses the card and requests to show all the cards from the specified expansion
		var card = JsonSerializer.Deserialize<ExpansionCard>(body, Common.JSON_SERIALIZATION_OPTIONS);

		LoadExpansion(card.Expansion);
	}

	private void OnCardContextMenuIndexPressed(int index)
	{
		var card = CardContextMenuNode.GetMeta("Card").As<Wrapper<ExpansionCard>>().Value;

		switch (index) {
		case 0:
			EditCard(card);
			break;
		case 1:
			DeleteCard(card);
			break;
		default:
			break;
		}
		// Replace with function body.
	}

	private void OnDeleteCardRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			var resp = Encoding.UTF8.GetString(body);

			CardAlertPopupNode.DialogText = $"Failed to delete card! (Response code: {response_code}\nResponse message:\n{resp}";
			CardAlertPopupNode.Show();
			return;
		}

		// TODO alert that successfully deleted card
		
		// * fetch the cards of the expansion of the deleted card
		var card = CardContextMenuNode.GetMeta("Card").As<Wrapper<ExpansionCard>>().Value;
		var expansion = card.Expansion;
		CardContextMenuNode.RemoveMeta("Card");
		
		LoadExpansion(expansion);
	}
	
	private void OnUpdateCardRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			var resp = Encoding.UTF8.GetString(body);

			CardAlertPopupNode.DialogText = $"Failed to update card! (Response code: {response_code}\nResponse message:\n{resp}";
			CardAlertPopupNode.Show();
			return;
		}

		CardEditWindowNode.Hide();

		var card = CardContextMenuNode.GetMeta("Card").As<Wrapper<ExpansionCard>>().Value;
		var expansion = card.Expansion;
		CardContextMenuNode.RemoveMeta("Card");
		
		LoadExpansion(expansion);
	}
	
	#endregion

	private void DeleteCard(ExpansionCard card) {
		// TODO confirm

		var baseUrl = GetNode<GlobalSettings>("/root/GlobalSettings").BaseUrl;
		var token = GetNode<GlobalSettings>("/root/GlobalSettings").JwtToken;

		string[] headers = new string[] { "Content-Type: application/json", $"Authorization: Bearer {token}" };
		DeleteCardRequestNode.Request(baseUrl + "card/" + card.GetCID(), headers, HttpClient.Method.Delete);
	}
	
	private void EditCard(ExpansionCard card) {
		CardEditNode.Load(card);
		CardEditWindowNode.Show();
	}
}



