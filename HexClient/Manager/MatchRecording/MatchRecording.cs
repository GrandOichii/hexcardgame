using Godot;
using HexCore.Cards;
using HexCore.Cards.Masters;
using HexCore.Decks;
using HexCore.GameMatch;
using HexCore.GameMatch.Players;
using HexCore.GameMatch.Players.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Utility;

namespace HexClient.Manager;
// !FIXME this will change to MatchRecord when a separate controller for match records will be created

public class ApiCardMaster : ICardMaster
{
	private readonly System.Net.Http.HttpClient _client;
	private readonly string _apiUrl;
	public ApiCardMaster(string apiUrl)
	{
		_apiUrl = apiUrl;
		_client = new();
	}

    public async Task<ExpansionCard> Get(string cid)
    {
		var request = await _client.GetAsync(_apiUrl + "card/" + Uri.EscapeDataString(cid));
		return await request.Content.ReadFromJsonAsync<ExpansionCard>();
    }

    public async Task<IEnumerable<ExpansionCard>> GetAll()
    {
		var request = await _client.GetAsync(_apiUrl + "card/");
		return await request.Content.ReadFromJsonAsync<List<ExpansionCard>>();
    }
}

public class QueuedActionPlayerController : IPlayerController
{
	public Queue<string> Actions;
	public QueuedActionPlayerController(PlayerRecord record)
	{
		Actions = new();
		foreach	(var action in record.Actions) {
			Actions.Enqueue(action);
		}
	}

	public Task CleanUp()
	{
		return Task.CompletedTask;
	}

	public Task<string> DoPickTile(List<int[]> choices, Player player, HexCore.GameMatch.Match match)
	{
		return Task.FromResult(
			Actions.Dequeue()
		);
	}

	public Task<string> DoPromptAction(Player player, HexCore.GameMatch.Match match)
	{
		return Task.FromResult(
			Actions.Dequeue()
		);
	}

	public Task SendCard(HexCore.GameMatch.Match match, Player player, ExpansionCard card)
	{
		return Task.CompletedTask;
	}

	public Task Setup(Player player, HexCore.GameMatch.Match match)
	{
		return Task.CompletedTask;
	}

	public Task Update(Player player, HexCore.GameMatch.Match match)
	{
		return Task.CompletedTask;
	}
}

public partial class MatchRecording : Control
{
	#region Nodes
	
	public Match.Match MatchNode { get; private set; }
	public Control OverlayNode { get; private set; }

	public HttpRequest FetchRecordRequestNode { get; private set; }
	public HttpRequest FetchConfigRequestNode { get; private set; }
	
	#endregion

	public string ApiUrl => GetNode<GlobalSettings>("/root/GlobalSettings").ApiUrl;
	
	public override void _Ready()
	{
		#region Node fetching
		
		MatchNode = GetNode<Match.Match>("%Match");
		OverlayNode = GetNode<Control>("%Overlay");
		
		FetchRecordRequestNode = GetNode<HttpRequest>("%FetchRecordRequest");
		FetchConfigRequestNode = GetNode<HttpRequest>("%FetchConfigRequest");
		
		#endregion
	}
	
	public void Load(string recordId) {
		FetchRecordRequestNode.Request(ApiUrl + "match/" + recordId);
	}

	#nullable enable
	private async Task<MatchConfig?> FetchConfig(string configId) {
	#nullable disable

		FetchConfigRequestNode.Request(ApiUrl + "config/" + configId);
		var results = await ToSignal(FetchConfigRequestNode, "request_completed");
		var body = results[3].As<byte[]>();
		var response_code = results[1].As<int>();

		if (response_code != 200) {
			var resp = Encoding.UTF8.GetString(body);
			// TODO show popup
			GD.Print(response_code);
			GD.Print(resp);
			return null;
		}

		var config = JsonSerializer.Deserialize<MatchConfig>(body, Common.JSON_SERIALIZATION_OPTIONS);
		return config;
	}

	private async Task CreateRecording(MatchRecord record) {
		var config = await FetchConfig(record.ConfigId);
		var match = new HexCore.GameMatch.Match("", config, new ApiCardMaster(ApiUrl), record.Seed);

		// TODO change
        match.InitialSetup("../HexCore/core.lua");        

		foreach (var p in record.Players) {
			var controller = new QueuedActionPlayerController(p);
			await match.AddPlayer(p.Name, DeckTemplate.FromText(p.Deck), controller);
		}

		await match.Start();

		// TODO continue here
		
	}

	
	#region Signal connections
	
	private void OnFetchRecordRequestRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		if (response_code != 200) {
			var resp = Encoding.UTF8.GetString(body);
			// TODO show popup
			GD.Print(response_code);
			GD.Print(resp);
			return;
		}

		var data = JsonSerializer.Deserialize<MatchProcess>(body, Common.JSON_SERIALIZATION_OPTIONS);
		_ = CreateRecording(data.Record);
	}

	#endregion
}

