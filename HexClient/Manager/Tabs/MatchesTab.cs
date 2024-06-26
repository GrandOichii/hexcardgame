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
	public string GetMatchId();
	public void Focus();
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
	public LineEdit PasswordEditNode { get; private set; }
	
	public HttpRequest CreateRequestNode { get; private set; }
	public HttpRequest FetchBasicConfigRequestNode { get; private set; }
	public HttpRequest RemoveCrashedRequestNode { get; private set; }

	public AcceptDialog FailedToCreatePopupNode { get; private set; }
	public AcceptDialog DeckErrorPopupNode { get; private set; }
	public AcceptDialog FailedToFetchBasicConfigPopupNode { get; private set; }

	public PlayerConfig PlayerConfig1Node { get; private set; }
	public PlayerConfig PlayerConfig2Node { get; private set; }
	
	#endregion
	
	public string ApiUrl {
		get => GetNode<GlobalSettings>("/root/GlobalSettings").ApiUrl;
		set => GetNode<GlobalSettings>("/root/GlobalSettings").ApiUrl = value;
	}

	public string BaseUrl {
		get => GetNode<GlobalSettings>("/root/GlobalSettings").BaseUrl;
		set => GetNode<GlobalSettings>("/root/GlobalSettings").BaseUrl = value;
	}
	
	public override void _Ready()
	{
		#region Node fetching

		MatchTableNode = GetNode<MatchTable>("%MatchTable");
		MatchConfigIdEditNode = GetNode<LineEdit>("%MatchConfigIdEdit");
		CanWatchCheckNode = GetNode<CheckBox>("%CanWatchCheck");
		BatchEditNode = GetNode<SpinBox>("%BatchEdit");
		WindowsNode = GetNode<Node>("%Windows");
		PasswordEditNode = GetNode<LineEdit>("%PasswordEdit");
		
		CreateRequestNode = GetNode<HttpRequest>("%CreateRequest");
		FetchBasicConfigRequestNode = GetNode<HttpRequest>("%FetchBasicConfigRequest");
		RemoveCrashedRequestNode = GetNode<HttpRequest>("%RemoveCrashedRequest");

		FailedToCreatePopupNode = GetNode<AcceptDialog>("%FailedToCreatePopup");
		DeckErrorPopupNode = GetNode<AcceptDialog>("%DeckErrorPopup");
		FailedToFetchBasicConfigPopupNode = GetNode<AcceptDialog>("%FailedToFetchBasicConfigPopup");

		PlayerConfig1Node = GetNode<PlayerConfig>("%PlayerConfig1");
		PlayerConfig2Node = GetNode<PlayerConfig>("%PlayerConfig2");

		#endregion

		GetNode<LineEdit>("%ApiUrlEdit").Text = ApiUrl;
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
			Password = PasswordEditNode.Text
		};

		return result;
	}

	#region Signal connections

	private void OnApiUrlEditTextChanged(string newText)
	{
		ApiUrl = newText;
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
			CreateRequestNode.Request(ApiUrl + "match/create", headers, HttpClient.Method.Post, data);

			var result = await ToSignal(CreateRequestNode, "request_completed");
			var responseCode = result[1].As<int>();
			var body = result[3].As<byte[]>();
			if (responseCode != 200) {
				var resp = Encoding.UTF8.GetString(body);
				FailedToCreatePopupNode.DialogText = $"Failed to create match! (code: {responseCode})\n\n{resp}";
				FailedToCreatePopupNode.Show();
				return;
			}
			var match = JsonSerializer.Deserialize<MatchProcess>(body, Common.JSON_SERIALIZATION_OPTIONS);
			OnMatchTableMatchActivated(new Wrapper<MatchProcess>(match));
		}
	}

	private void OnLiveMatchesButtonPressed()
	{
		_ = MatchTableNode.Connect(ApiUrl + "match/live");
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
		FetchBasicConfigRequestNode.Request(ApiUrl + "config/basic");
	}

	private void OnRemoveCrashedButtonPressed()
	{
		var token = GetNode<GlobalSettings>("/root/GlobalSettings").JwtToken;
		string[] headers = new string[] { "Content-Type: application/json", $"Authorization: Bearer {token}" };

		RemoveCrashedRequestNode.Request(ApiUrl + "match/crashed", headers, HttpClient.Method.Delete);
	}

	private void OnRemoveCrashedRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code == 200) return;

		// TODO show popup
		GD.Print(response_code);
	}

	private void OnMatchTableMatchActivated(Wrapper<MatchProcess> match)
	{
		foreach (var c in WindowsNode.GetChildren()) {
			if (c is not IMatchProcessViewWindow w) continue;

			if (w.GetMatchId() == match.Value.Id.ToString()) {
				w.Focus();
				return;
			}
		}

		var child = MatchProcessViewWindowPS.Instantiate() as Window;
		WindowsNode.AddChild(child);
		
		var window = child as IMatchProcessViewWindow;
		window.Load(match.Value.Id.ToString());
	}

	private void OnBaseUrlEditTextChanged(string newText)
	{
		BaseUrl = newText;
	}

	#endregion
}


