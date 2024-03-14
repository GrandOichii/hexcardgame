using Godot;
using System;
using System.Text.Json;

namespace HexClient.Manager;

public partial class DeckCardDisplay : Control, IDeckCardDisplay
{
	#region Nodes

	public Card CardNode { get; private set; }
	public Label CIDLabelNode { get; private set; }
	public Label AmountLabelNode { get; private set; }
	public HttpRequest FetchCardRequestNode { get; private set; }

	#endregion

	public override void _Ready()
	{
		#region Node fetching

		CardNode = GetNode<Card>("%Card");
		CIDLabelNode = GetNode<Label>("%CIDLabel");
		AmountLabelNode = GetNode<Label>("%AmountLabel");
		FetchCardRequestNode = GetNode<HttpRequest>("%FetchCardRequest");

		#endregion

		CardNode.Hide();
	}

	public void Load(string cid, int amount)
	{
		CIDLabelNode.Text = cid;
		AmountLabelNode.Text = amount.ToString();
		
		var baseUrl = GetNode<GlobalSettings>("/root/GlobalSettings").BaseUrl;
		FetchCardRequestNode.Request(baseUrl + "card/" + Uri.EscapeDataString(cid));
	}
	
	#region Signal connections
	
	private void OnFetchCardRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		// TODO check response code
		var card = JsonSerializer.Deserialize<HexCore.Cards.ExpansionCard>(body);
		CardNode.Load(card);
		CardNode.Show();
	}
	
	#endregion
}
