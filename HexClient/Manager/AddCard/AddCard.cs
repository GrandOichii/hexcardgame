using Godot;
using System;

namespace HexClient.Manager;

// TODO make more complex and user friendly

public partial class AddCard : Control
{
	#region Signals
	
	[Signal]
	public delegate void AddedEventHandler(string cid);
	
	#endregion

	#region Nodes

	public LineEdit CIDEditNode { get; private set; }

	public HttpRequest FetchCardRequestNode { get; private set; }
	
	public AcceptDialog CardNotFoundPopupNode { get; private set; }

	#endregion
	
	public override void _Ready()
	{
		#region Node fetching
		
		CIDEditNode = GetNode<LineEdit>("%CIDEdit");

		FetchCardRequestNode = GetNode<HttpRequest>("%FetchCardRequest");
		
		CardNotFoundPopupNode = GetNode<AcceptDialog>("%CardNotFoundPopup");
		
		#endregion
	}

	public string BaseUrl => GetNode<GlobalSettings>("/root/GlobalSettings").ApiUrl;

	
	#region Signal connections
	
	private void OnAddButtonPressed()
	{
		FetchCardRequestNode.Request(BaseUrl + "card/" + Uri.EscapeDataString(CIDEditNode.Text));
	}

	private void OnFetchCardRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			CardNotFoundPopupNode.DialogText = $"Card {CIDEditNode.Text} not found";
			CardNotFoundPopupNode.Show();
			
			return;
		}
		
		var cid = CIDEditNode.Text;
		CIDEditNode.Text = "";
		EmitSignal(SignalName.Added, cid);
	}
	
	#endregion
}



