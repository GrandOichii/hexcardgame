using Godot;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Utility;

namespace HexClient.Tables;

public partial class MatchTable : Control
{
	#region Signals

	[Signal]
	public delegate void MatchActivatedEventHandler(Wrapper<MatchProcess> match);

	#endregion

	#region Node

	public Tree MatchTreeNode { get; private set; }
	public AcceptDialog FailedLiveConnectPopupNode { get; private set; }
	public AcceptDialog DisconnectedPopupNode { get; private set; }

	#endregion

	private TreeItem _root;

	public override void _Ready()
	{
		#region Node fetching

		MatchTreeNode = GetNode<Tree>("%MatchTree");
		
		FailedLiveConnectPopupNode = GetNode<AcceptDialog>("%FailedLiveConnectPopup");
		DisconnectedPopupNode = GetNode<AcceptDialog>("%DisconnectedPopup");

		#endregion

		_root = MatchTreeNode.CreateItem();
		_root.SetText(0, "ID");
		_root.SetText(1, "Status");
		_root.SetText(2, "Winner");
		_root.SetText(3, "Tcp");
	}
	
	public async Task Connect(string url) {
		while (_root.GetChildCount() > 0)  {
			_root.RemoveChild(_root.GetFirstChild());
		}

		var connection = new HubConnectionBuilder()
			.WithUrl(url)
			.Build();

		connection.On<string>("Update", OnUpdate);
		connection.On<string>("UpdateAll", OnUpdateAll);
		connection.Closed += OnConnectionClosed;

		try {
			await connection.StartAsync();
			await connection.SendAsync("Get");
		} catch (Exception) {
			FailedLiveConnectPopupNode.Show();
		}
	}

	private Task OnUpdate(string message) {
		var match = JsonSerializer.Deserialize<MatchProcess>(message, Common.JSON_SERIALIZATION_OPTIONS);

		CallDeferred("ProcessMatchData", new Wrapper<MatchProcess>(match));
		return Task.CompletedTask;
	}

	private Task OnUpdateAll(string message) {
		var matches = JsonSerializer.Deserialize<List<MatchProcess>>(message, Common.JSON_SERIALIZATION_OPTIONS);
		
		CallDeferred("SetData", new Wrapper<List<MatchProcess>>(matches));
		return Task.CompletedTask;
	}

	private void SetData(Wrapper<List<MatchProcess>> matches) {
		// remove existing
		while (_root.GetChildCount() > 0) {
			_root.RemoveChild(_root.GetFirstChild());
		}

		foreach (var m in matches.Value) {
			ProcessMatchData(new Wrapper<MatchProcess>(m));
		}
	}

	private Task OnConnectionClosed(Exception e) {
		DisconnectedPopupNode.CallDeferred("set_text", $"Disconnected from live matches!\n\n{e.Message}");
		DisconnectedPopupNode.CallDeferred("show");
		
		GD.Print(e.Message);
		GD.Print(e.StackTrace);
		GD.Print("=====");
		GD.Print(e.InnerException.Message);
		GD.Print(e.InnerException.StackTrace);

		return Task.CompletedTask;
	}

	private void ProcessMatchData(Wrapper<MatchProcess> matchW) {
		var existing = _root.GetChildren().FirstOrDefault(c => c.GetMeta("MatchId").AsString() == matchW.Value.Id.ToString());
		
		var target = existing ?? _root.CreateChild();

		SetMatchData(target, matchW.Value);
	}

	private static void SetMatchData(TreeItem item, MatchProcess match) {
		item.SetMeta("MatchId", match.Id.ToString());
		item.SetMeta("Match", new Wrapper<MatchProcess>(match));

		item.SetText(0, match.Id.ToString()[..3]);
		item.SetText(1, match.Status.ToFriendlyString());
		item.SetText(2, match.Record.WinnerName ?? "");
		item.SetText(3, match.TcpAddress);
	}

	#region Signal connections

	private void OnMatchTreeItemActivated()
	{
		var row = MatchTreeNode.GetSelected();
		var col = MatchTreeNode.GetSelectedColumn();

		var match = row.GetMeta("Match").As<Wrapper<MatchProcess>>();

		EmitSignal(SignalName.MatchActivated, match);
	}
	
	#endregion
}

