using Godot;
using HexCore.Cards;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Utility;

namespace HexClient.Client.Tabs;

public struct Expansion {
	public string Name { get; set; }
	public int CardCount { get; set; }
}

public interface ICardDisplay {
	public void Load(ExpansionCard card);
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

	public HttpRequest FetchExpansionsRequestNode { get; private set; }
	public HttpRequest FetchExpansionCardsRequestNode { get; private set; }
	
	#endregion
	
	public override void _Ready()
	{
		#region Node fetching
		
		ExpansionsListNode = GetNode<ItemList>("%ExpansionsList");
		CardsContainerNode = GetNode<FlowContainer>("%CardsContainer");

		FetchExpansionsRequestNode = GetNode<HttpRequest>("%FetchExpansionsRequest");
		FetchExpansionCardsRequestNode = GetNode<HttpRequest>("%FetchExpansionCardsRequest");
		
		#endregion
	}

	private void UpdateExpansion(Expansion expansion) {
		// TODO check existing

		var itemI = ExpansionsListNode.AddItem($"{expansion.Name} ({expansion.CardCount})");
		ExpansionsListNode.SetItemMetadata(itemI, new Wrapper<Expansion>(expansion));
	}

	private void LoadExpansion(Expansion expansion) {
		while (CardsContainerNode.GetChildCount() > 0)
			CardsContainerNode.RemoveChild(CardsContainerNode.GetChild(0));

		var baseUrl = GetNode<GlobalSettings>("/root/GlobalSettings").BaseUrl;
		FetchExpansionCardsRequestNode.Request(baseUrl + "card/fromexpansion/" + expansion.Name);
	}

	#region Signal connections
	
	private void OnExpansionsListItemActivated(int index)
	{
		// TODO
		var expansion = ExpansionsListNode.GetItemMetadata(index).As<Wrapper<Expansion>>().Value;

		LoadExpansion(expansion);
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
		}
	}
	
	#endregion
}


