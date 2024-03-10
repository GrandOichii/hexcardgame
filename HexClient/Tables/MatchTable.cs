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
	#region Node

	public Tree MatchTreeNode { get; private set; }
	public AcceptDialog FailedLiveConnectPopupNode { get; private set; }

	#endregion

	private TreeItem _root;

	public override void _Ready()
	{
		#region Node fetching

		MatchTreeNode = GetNode<Tree>("%MatchTree");
		
		FailedLiveConnectPopupNode = GetNode<AcceptDialog>("%FailedLiveConnectPopup");

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

	private Task OnConnectionClosed(Exception e) {
		// TODO
		GD.Print("connection closed");
		return Task.CompletedTask;
	}

	private void ProcessMatchData(Wrapper<MatchProcess> matchW) {
		var existing = _root.GetChildren().FirstOrDefault(c => c.GetText(0) == matchW.Value.Id.ToString());
		
		var target = existing ?? _root.CreateChild();

		SetMatchData(target, matchW.Value);
	}

	private static void SetMatchData(TreeItem item, MatchProcess match) {
		item.SetText(0, match.Id.ToString());
		item.SetText(1, match.Status.ToFriendlyString());
		item.SetText(2, match.Record.WinnerName ?? "");
		item.SetText(3, match.TcpAddress);
	}
}
