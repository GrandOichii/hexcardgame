using Godot;
using System;
using System.IO;
using System.Text.Json;
using Utility;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using HexClient.Manager.Tables;
using System.Text;
using HexCore.Decks;
using System.Collections.Generic;

namespace HexClient.Manager.Tabs;

public interface IMatchProcessViewWindow {
	public void Load(string matchId);
}

public partial class MatchesTab : Control
{
	#region Packed scenes

	[Export]
	private PackedScene MatchViewWindowPS { get; set; }
	[Export]
	private PackedScene ConnectedMatchWindowPS { get; set; }
	[Export]
	private PackedScene MatchProcessViewWindowPS { get; set; }

	#endregion

	#region Nodes
	
	public MatchTable MatchTableNode { get; private set; }
	public LineEdit MatchConfigIdEditNode { get; private set; }
	public CheckBox CanWatchCheckNode { get; private set; }
	public SpinBox BatchEditNode { get; private set; }
	public Node WindowsNode { get; private set; }
	
	public HttpRequest CreateRequestNode { get; private set; }
	public HttpRequest FetchBasicConfigRequestNode { get; private set; }
	public HttpRequest RemoveCrashedRequestNode { get; private set; }

	public AcceptDialog FailedToCreatePopupNode { get; private set; }
	public AcceptDialog DeckErrorPopupNode { get; private set; }
	public AcceptDialog FailedToFetchBasicConfigPopupNode { get; private set; }

	public PlayerConfig PlayerConfig1Node { get; private set; }
	public PlayerConfig PlayerConfig2Node { get; private set; }
	
	#endregion
	
	public string BaseUrl {
		get => GetNode<GlobalSettings>("/root/GlobalSettings").ApiUrl;
		set => GetNode<GlobalSettings>("/root/GlobalSettings").ApiUrl = value;
	}
	
	public override void _Ready()
	{
		#region Node fetching

		MatchTableNode = GetNode<MatchTable>("%MatchTable");
		MatchConfigIdEditNode = GetNode<LineEdit>("%MatchConfigIdEdit");
		CanWatchCheckNode = GetNode<CheckBox>("%CanWatchCheck");
		BatchEditNode = GetNode<SpinBox>("%BatchEdit");
		WindowsNode = GetNode<Node>("%Windows");
		
		CreateRequestNode = GetNode<HttpRequest>("%CreateRequest");
		FetchBasicConfigRequestNode = GetNode<HttpRequest>("%FetchBasicConfigRequest");
		RemoveCrashedRequestNode = GetNode<HttpRequest>("%RemoveCrashedRequest");

		FailedToCreatePopupNode = GetNode<AcceptDialog>("%FailedToCreatePopup");
		DeckErrorPopupNode = GetNode<AcceptDialog>("%DeckErrorPopup");
		FailedToFetchBasicConfigPopupNode = GetNode<AcceptDialog>("%FailedToFetchBasicConfigPopup");

		PlayerConfig1Node = GetNode<PlayerConfig>("%PlayerConfig1");
		PlayerConfig2Node = GetNode<PlayerConfig>("%PlayerConfig2");

		#endregion

		GetNode<LineEdit>("%BaseUrlEdit").Text = BaseUrl;

		PlayerConfig1Node.BotNameEditNode.Text += "1";
		PlayerConfig2Node.BotNameEditNode.Text += "2";

		OnLiveMatchesButtonPressed();
		OnFetchBasicConfigButtonPressed();
	}

	private MatchProcessConfig BuildCreateMatchProcessConfig() {

		var p1Config = PlayerConfig1Node.Baked;
		PlayerConfig1Node.ActionDelaySpinNode.Value = 0;

		var p2Config = PlayerConfig2Node.Baked;
		PlayerConfig2Node.ActionDelaySpinNode.Value = 0;
		
		var result = new MatchProcessConfig
		{
			MatchConfigId = MatchConfigIdEditNode.Text,
			CanWatch = CanWatchCheckNode.ButtonPressed,
			P1Config = p1Config,
			P2Config = p2Config,
		};

		return result;
	}

	#region Signal connections

	private void OnBaseUrlEditTextChanged(string newText)
	{
		BaseUrl = newText;
	}

	private async void OnCreateMatchButtonPressed()
	{
		for (int i = 0; i < BatchEditNode.Value; i++) {
			MatchProcessConfig config;
			try {
				config = BuildCreateMatchProcessConfig();
			} catch (DeckParseException e) {
				DeckErrorPopupNode.DialogText = $"Failed to load deck file!\n\n{e.Message}";
				DeckErrorPopupNode.Show();

				return;
			} catch (FileNotFoundException e) {
				DeckErrorPopupNode.DialogText = $"Failed to load deck file!\n\n{e.Message}";
				DeckErrorPopupNode.Show();

				return;
			}
			
			var token = GetNode<GlobalSettings>("/root/GlobalSettings").JwtToken;
			string[] headers = new string[] { "Content-Type: application/json", $"Authorization: Bearer {token}" };
			var data = JsonSerializer.Serialize(config, Common.JSON_SERIALIZATION_OPTIONS);
			GD.Print(data);
			CreateRequestNode.Request(BaseUrl + "match/create", headers, HttpClient.Method.Post, data);
			await ToSignal(CreateRequestNode, "request_completed");
		}
	}

	private void OnCreateRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			var resp = Encoding.UTF8.GetString(body);
			FailedToCreatePopupNode.DialogText = $"Failed to create match! (code: {response_code})\n\n{resp}";
			FailedToCreatePopupNode.Show();

			return;
		}

	}

	private void OnLiveMatchesButtonPressed()
	{
		_ = MatchTableNode.Connect(BaseUrl + "match/live");
	}

	private void OnFetchBasicConfigRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			var resp = Encoding.UTF8.GetString(body);
			FailedToFetchBasicConfigPopupNode.DialogText = $"Failed to fetch basic match config! (code: {response_code})\n\n{resp}";
			FailedToFetchBasicConfigPopupNode.Show();

			return;
		}
		
		var configId = Encoding.UTF8.GetString(body);
		MatchConfigIdEditNode.Text = configId;
	}

	private void OnFetchBasicConfigButtonPressed()
	{
		FetchBasicConfigRequestNode.Request(BaseUrl + "config/basic");
	}

	private void OnRemoveCrashedButtonPressed()
	{
		var token = GetNode<GlobalSettings>("/root/GlobalSettings").JwtToken;
		string[] headers = new string[] { "Content-Type: application/json", $"Authorization: Bearer {token}" };

		RemoveCrashedRequestNode.Request(BaseUrl + "match/crashed", headers, HttpClient.Method.Delete);
	}

	private void OnRemoveCrashedRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code == 200) return;

		// TODO show popup
		GD.Print(response_code);
	}

	private void OnMatchTableMatchActivated(Wrapper<MatchProcess> match)
	{
		// TODO? this allows to view the same record from 2 different windows, change?

		var child = MatchProcessViewWindowPS.Instantiate() as Window;
		WindowsNode.AddChild(child);
		
		var window = child as IMatchProcessViewWindow;
		window.Load(match.Value.Id.ToString());
	}

	#endregion
}

