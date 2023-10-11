using core.decks;
using Godot;
using System;
using core.manager;
using System.Collections.Generic;
using core.manager;
using System.Text.Json;

public partial class MatchesTab : Control
{
	#region Packed scenes
	
	private readonly static PackedScene MatchPlayerPS = ResourceLoader.Load<PackedScene>("res://Manager/MatchPlayer.tscn");
	
	#endregion
	
	#region Nodes
	
	public VBoxContainer PlayerContainerNode { get; private set; }
	public Tree MatchTracesNode { get; private set; }
	public SpinBox BatchSpinNode { get; private set; }
	public OptionButton ConfigOptionNode { get; private set; }
	public HttpRequest NewMatchRequestNode { get; private set; }
	public LineEdit SeedEditNode { get; private set; }
	public HttpRequest MatchesRequestNode { get; private set; }
	
	#endregion

	private string _url;
	private bool _requestCompleted = true;
	
	public override void _Ready()
	{
		#region Node fetching
		
		PlayerContainerNode = GetNode<VBoxContainer>("%PlayerContainer");
		MatchTracesNode = GetNode<Tree>("%MatchTraces");
		BatchSpinNode = GetNode<SpinBox>("%BatchSpin");
		ConfigOptionNode = GetNode<OptionButton>("%ConfigOption");
		NewMatchRequestNode = GetNode<HttpRequest>("%NewMatchRequest");
		SeedEditNode = GetNode<LineEdit>("%SeedEdit");
		MatchesRequestNode = GetNode<HttpRequest>("%MatchesRequest");
		
		#endregion
		
		// populate players
		for (int i = 0; i < 2; i++) {
			var child = MatchPlayerPS.Instantiate() as MatchPlayer;
			PlayerContainerNode.AddChild(child);
			child.NameEditNode.Text = "P" + (i+1); 
		}
		
	}
	
	public void UpdatePlayers(List<DeckTemplate> decks) {
		foreach (var child in PlayerContainerNode.GetChildren()) {
			var p = child as MatchPlayer;
			p.UpdateDecks(decks);
		}
	}
	
	#region Signal connections

	private void _on_decks_decks_updated(Wrapper<List<DeckTemplate>> decksW)
	{
		UpdatePlayers(decksW.Value);
	}

	private void _on_start_match_button_pressed()
	{
		if (ConfigOptionNode.GetSelectableItem() == -1) {
			GUtil.Alert(this, "Pick match configuration first", "Manager");
			return;
		}

		var config = new MatchCreationConfig
		{
			Seed = SeedEditNode.Text,
			Batch = (int)BatchSpinNode.Value,
			Config = ConfigOptionNode.GetSelectedMetadata().As<Wrapper<ManagerMatchConfig>>().Value.Config,
			Players = new()
		};
		foreach (var child in PlayerContainerNode.GetChildren()) {
			var c = child as MatchPlayer;
			config.Players.Add(c.Baked);
		}

		var t = config.ToJson();
		string[] headers = new string[] { "Content-Type: application/json" };
		NewMatchRequestNode.Request(_url + "/api/Matches", headers, HttpClient.Method.Post, t);
	}
	
	private void _on_configurations_configs_updated(Wrapper<List<ManagerMatchConfig>> configsW)
	{
		ConfigOptionNode.Clear();
		foreach (var config in configsW.Value) {
			ConfigOptionNode.AddItem(config.Name);
			ConfigOptionNode.SetItemMetadata(ConfigOptionNode.ItemCount - 1, new Wrapper<ManagerMatchConfig>(config));
		}
	}

	private void _on_manager_url_updated(string url)
	{
		_url = url;
	}

	private void _on_new_match_request_request_completed(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			GUtil.Alert(this, "Failed to create a new match (response code: " + response_code + ")", "Manager");
			return;
		}
		
		
		// Replace with function body.
	}

	private void _on_poll_matches_timer_timeout()
	{
		if (!_requestCompleted) return;
		MatchesRequestNode.Request(_url + "/api/Matches");
		_requestCompleted = false;
	}

	private void _on_matches_request_request_completed(long result, long response_code, string[] headers, byte[] body)
	{
		_requestCompleted = true;
		if (response_code != 200) {
			return;
		}
		
		var text = System.Text.Encoding.Default.GetString(body);
		var matches = JsonSerializer.Deserialize<List<MatchTrace>>(text);
		GD.Print(matches.Count);
	}
	
	#endregion
}

