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
	public Control ErrorOverlayNode { get; private set; }
	public Label ErrorLabelNode { get; private set; }

	#endregion

	public string CID { get; private set; }
	public bool Valid { get; private set; } = false;

	public int Amount {
		get => int.Parse(AmountLabelNode.Text);
		set => AmountLabelNode.Text = value.ToString();
	}

	public override void _Ready()
	{
		#region Node fetching

		CardNode = GetNode<Card>("%Card");
		CIDLabelNode = GetNode<Label>("%CIDLabel");
		AmountLabelNode = GetNode<Label>("%AmountLabel");
		FetchCardRequestNode = GetNode<HttpRequest>("%FetchCardRequest");
		ErrorOverlayNode = GetNode<Control>("%ErrorOverlay");
		ErrorLabelNode = GetNode<Label>("%ErrorLabel");

		#endregion

		ErrorOverlayNode.Hide();
		CardNode.Hide();
	}

	public void Load(string cid, int amount)
	{
		CID = cid;
		ErrorOverlayNode.Hide();

		CIDLabelNode.Text = cid;
		Amount = amount;
		
		var baseUrl = GetNode<GlobalSettings>("/root/GlobalSettings").BaseUrl;
		FetchCardRequestNode.CancelRequest();
		FetchCardRequestNode.Request(baseUrl + "card/" + Uri.EscapeDataString(cid));
	}
	
	#region Signal connections
	
	private void OnFetchCardRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			
			if (response_code == 404) {
				// card not found
				Valid = false;
				ErrorLabelNode.Text = $"Inexistant card\n{CID}";
				ErrorOverlayNode.Show();
				return;
			}
			return;
		}
		var card = JsonSerializer.Deserialize<HexCore.Cards.ExpansionCard>(body);
		CardNode.Load(card);
		CardNode.Show();
		Valid = true;
	}
	
	#endregion
}
