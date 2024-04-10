using Godot;
using HexCore.Cards;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Utility;

namespace HexClient.Manager;

// TODO make more complex and user friendly

public partial class AddCard : Control
{
	#region Signals
	
	[Signal]
	public delegate void AddedEventHandler(string cid);
	
	#endregion

	#region Nodes

	public Cards.Card CardNode { get; private set; }
	public LineEdit CIDEditNode { get; private set; }
	public ItemList CIDListNode { get; private set; }

	public HttpRequest FetchCardRequestNode { get; private set; }
	public HttpRequest FetchCIDsRequestNode { get; private set; }
	
	public AcceptDialog CardNotFoundPopupNode { get; private set; }

	#endregion

	public string ApiUrl => GetNode<GlobalSettings>("/root/GlobalSettings").ApiUrl;
	private List<string> _cids;
	
	public override void _Ready()
	{
		#region Node fetching
		
		CardNode = GetNode<Cards.Card>("%Card");
		CIDEditNode = GetNode<LineEdit>("%CIDEdit");
		CIDListNode = GetNode<ItemList>("%CIDList");

		FetchCardRequestNode = GetNode<HttpRequest>("%FetchCardRequest");
		FetchCIDsRequestNode = GetNode<HttpRequest>("%FetchCIDsRequest");
		
		CardNotFoundPopupNode = GetNode<AcceptDialog>("%CardNotFoundPopup");
		
		#endregion
	}


	public void RefetchCIDs() {
		FetchCIDsRequestNode.Request(ApiUrl + "card/cid/all");
	}

	
	#region Signal connections

	private void OnFetchCardRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			CardNotFoundPopupNode.DialogText = $"Card {CIDEditNode.Text} not found";
			CardNotFoundPopupNode.Show();
			
			return;
		}
		
		var card = JsonSerializer.Deserialize<ExpansionCard>(body, Common.JSON_SERIALIZATION_OPTIONS);
		CardNode.Load(card);
	}

	private void OnCidListItemSelected(int index)
	{
		CIDEditNode.Text = CIDListNode.GetItemText(index);
		FetchCardRequestNode.Request(ApiUrl + "card/" + Uri.EscapeDataString(CIDEditNode.Text));
	}

	private void OnCidListItemActivated(int index)
	{
		var cid = CIDListNode.GetItemText(index);
		CIDEditNode.Text = "";
		EmitSignal(SignalName.Added, cid);	
	}
	
	private void OnCidEditTextChanged(string new_text)
	{
		CIDListNode.Clear();
		foreach (var cid in _cids) {
			if (cid.ToLower().Contains(new_text))
				CIDListNode.AddItem(cid);
		}
	}

	private void OnFetchCiDsRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			var resp = Encoding.UTF8.GetString(body);
			// TODO show popup
			GD.Print(response_code);
			GD.Print(resp);
			return;
		}

		_cids = JsonSerializer.Deserialize<List<string>>(body);
		CIDListNode.Clear();
		foreach (var cid in _cids) {
			CIDListNode.AddItem(cid);
		}
	}
	
	#endregion
}



