using Godot;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Utility;

namespace HexClient.Tables;

public partial class MatchTable : Control
{
	#region Node

	public Tree MatchTreeNode { get; private set; }

	#endregion

	private TreeItem _root;

	public override void _Ready()
	{
		#region Node fetching

		MatchTreeNode = GetNode<Tree>("%MatchTree");

		#endregion

		_root = MatchTreeNode.CreateItem();
		_root.SetText(0, "ID");
		_root.SetText(1, "Status");
		// _root.SetText(2, "Winner");
		// _root.SetText(2, "Availability");
		_root.SetText(2, "Tcp");
	}
	
	public async Task Connect(string url) {
		// TODO clear all current items

		var connection = new HubConnectionBuilder()
			.WithUrl(url)
			.Build();
		GD.Print(url);

		connection.On<string>("Update", OnUpdate);
		connection.Closed += OnConnectionClosed;

		// TODO
		try {
			await connection.StartAsync();
			await connection.SendAsync("Get");
		} catch (Exception e) {
			// TODO
			GD.Print("Failed to connect");
			GD.Print(e.Message);
		}
	}

	private async Task OnUpdate(string message) {
		var match = JsonSerializer.Deserialize<MatchProcess>(message, Common.JSON_SERIALIZATION_OPTIONS);

		CallDeferred("ProcessMatchData", new Wrapper<MatchProcess>(match));
	}

	private async Task OnConnectionClosed(Exception e) {
		// TODO
		GD.Print("connection closed");
	}

	private void ProcessMatchData(Wrapper<MatchProcess> matchW) {
		// TODO update existing
		var item = _root.CreateChild();
		SetMatchData(item, matchW.Value);
	}

	private void SetMatchData(TreeItem item, MatchProcess match) {
		item.SetText(0, match.Id.ToString());
		item.SetText(1, match.Status.ToString());
		item.SetText(2, match.TcpAddress);
	}
}
