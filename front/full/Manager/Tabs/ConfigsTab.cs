using Godot;
using System;
using System.Collections.Generic;

using core.match;
using core.manager;
using System.Text.Json;

public partial class ConfigsTab : Control
{
	#region Signals
	
	[Signal]
	public delegate void ConfigsUpdatedEventHandler(Wrapper<List<MatchConfig>> configsW);
	
	#endregion

	#region Nodes

	public HttpRequest RequestNode { get; private set; }

	#endregion
	
	private string _url;

	public override void _Ready()
	{
		#region Node fetching

		RequestNode = GetNode<HttpRequest>("%Request");

		#endregion


	}

	public override void _Input(InputEvent e)
	{
		if (e.IsActionPressed("manager-refresh"))
			Refresh();
	}

	public void Refresh() {
		if (_url.Length == 0) {
			GUtil.Alert(this, "Enter backend URL", "Manager");
			return;
		}
		
		RequestNode.Request(_url + "/api/Configs");
	}

	#region Signal connections

	private void _on_request_request_completed(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			GUtil.Alert(this, "Failed to fetch configs data (response code: " + response_code + ")", "Manager");
			return;
		}
		
		var text = System.Text.Encoding.Default.GetString(body);
		var configs = JsonSerializer.Deserialize<List<ManagerMatchConfig>>(text);
		
		EmitSignal(SignalName.ConfigsUpdated, new Wrapper<List<ManagerMatchConfig>>(configs));
	}

	private void _on_manager_url_updated(string url)
	{
		_url = url;
	}

	#endregion
}


