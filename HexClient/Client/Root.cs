using Godot;
using System;
using System.IO;
using System.Text.Json;
using Shared;
using Utility;
using Microsoft.AspNetCore.SignalR.Client;
using HexClient.Match.View;
using HexClient.Utility;

namespace HexClient.Client;

public enum MatchStatus {
	WAITING_FOR_PLAYERS,
	IN_PROGRESS,
	FINISHED,
	CRASHED
}

public class MatchProcess {
	public MatchStatus Status { get; set; }
	// public MatchRecord? Record { get; private set; } = null;
	public string TcpAddress { get; set; }
	public Guid Id { get; set; }
}

public partial class Root : Control
{
	private readonly static PackedScene MatchViewWindowPS = ResourceLoader.Load<PackedScene>("res://Match/View/MatchViewWindow.tscn");

	#region Nodes
	
	public LineEdit ConnectMatchIdEditNode { get; private set; }
	public LineEdit WatchMatchIdEditNode { get; private set; }
	public HttpRequest CreateRequestNode { get; private set; }
	public HttpRequest ConnectRequestNode { get; private set; }
	public CheckBox IsBotCheckNode { get; private set; }
	public Node WindowsNode { get; private set; }
	
	#endregion
	
	public string BaseUrl {
		get => GetNode<GlobalSettings>("/root/GlobalSettings").BaseUrl;
		set => GetNode<GlobalSettings>("/root/GlobalSettings").BaseUrl = value;
	}
	
	public override void _Ready()
	{
		#region Node fetching
		
		WindowsNode = GetNode<Node>("%Windows");
		ConnectMatchIdEditNode = GetNode<LineEdit>("%ConnectMatchIdEdit");
		WatchMatchIdEditNode = GetNode<LineEdit>("%WatchMatchIdEdit");
		IsBotCheckNode = GetNode<CheckBox>("%IsBotCheck");

		ConnectRequestNode = GetNode<HttpRequest>("%ConnectRequest");
		CreateRequestNode = GetNode<HttpRequest>("%CreateRequest");
		
		#endregion
		
		OnBaseUrlEditTextChanged(GetNode<LineEdit>("%BaseUrlEdit").Text);
	}

	#region Signal connection

	private void OnConnectButtonPressed()
	{
		// TODO

	}

	private void OnBaseUrlEditTextChanged(string newText)
	{
		BaseUrl = newText;
	}

	private void OnCreateMatchButtonPressed()
	{
		string[] headers = new string[] { "Content-Type: application/json" };
		var data = IsBotCheckNode.ButtonPressed ? File.ReadAllText("real.json") : File.ReadAllText("bot.json");
		CreateRequestNode.Request(BaseUrl + "match/create", headers, HttpClient.Method.Post, data);
	}

	private void OnCreateRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			// TODO show message box
			GD.Print("Error! Response code: " + response_code);
			return;
		}

		var match = JsonSerializer.Deserialize<MatchProcess>(body, Common.JSON_SERIALIZATION_OPTIONS);
		GD.Print(match.Id);
	}

	private void OnConnectRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		// TODO
	}

	private void OnWatchButtonPressed()
	{
		var connection = new HubConnectionBuilder()
			.WithUrl(BaseUrl + "/match/watch")
			.Build();

		var window = MatchViewWindowPS.Instantiate() as MatchViewWindow;
		WindowsNode.AddChild(window);
		window.Connect(connection, WatchMatchIdEditNode.Text);
	}
	
	#endregion
}

