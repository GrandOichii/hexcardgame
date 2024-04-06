using Godot;
using HexCore.Cards;
using HexCore.Cards.Masters;
using HexCore.Decks;
using HexCore.GameMatch;
using HexCore.GameMatch.Players;
using HexCore.GameMatch.Players.Controllers;
using HexCore.GameMatch.View;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Utility;

namespace HexClient.Manager;

// !FIXME this will change to MatchRecord when a separate controller for match records will be created

public class ConsoleLogger : ILogger
{
	public IDisposable BeginScope<TState>(TState state) where TState : notnull => default!;

	public bool IsEnabled(LogLevel logLevel) => true;
		// getCurrentConfig().LogLevelToColorMap.ContainsKey(logLevel);

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
	{
		GD.Print(formatter(state, exception));
	}
}

public partial class ApiCardMaster : ICardMaster
{
	private readonly string _apiUrl;
	private readonly HttpRequest _requestNode;
	public ApiCardMaster(string apiUrl, HttpRequest cardFetchRequestNode)
	{
		_requestNode = cardFetchRequestNode;	
		_apiUrl = apiUrl;
	}

	public async Task<ExpansionCard> Get(string cid)
	{
		var client = new System.Net.Http.HttpClient();

		var url = _apiUrl + "card/" + Uri.EscapeDataString(cid);
		var data = await client.GetAsync(url);
		var str = await data.Content.ReadAsStringAsync();
		var card = JsonSerializer.Deserialize<ExpansionCard>(str, Common.JSON_SERIALIZATION_OPTIONS);
		return card;
	}
}

public class QueuedActionPlayerController : IPlayerController
{
	public Queue<string> Actions { get; }
	private readonly ActionAggregate _aggregate;
	private readonly PlayerRecord _record;
	public QueuedActionPlayerController(PlayerRecord record, ActionAggregate aggregate)
	{
		_aggregate = aggregate;
		_record = record;

		Actions = new();
		foreach (var action in record.Actions)
		{
			Actions.Enqueue(action);
		}
	}

	public Task CleanUp()
	{
		return Task.CompletedTask;
	}

	public Task<string> DoPickTile(List<int[]> choices, Player player, HexCore.GameMatch.Match match)
	{
		var result = Actions.Dequeue();

		_aggregate.Actions.Add(new() {
			PlayerName = _record.Name,
			Action = result
		});

		return Task.FromResult(
			result
		);
	}

	public Task<string> DoPromptAction(Player player, HexCore.GameMatch.Match match)
	{
		var result = Actions.Dequeue();

		_aggregate.Actions.Add(new() {
			PlayerName = _record.Name,
			Action = result
		});
		
		return Task.FromResult(
			result
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

public class RecordingMatchView : IMatchView
{
	public delegate Task MatchEnd();

	#nullable enable
	public event MatchEnd? MatchEnded;
	#nullable disable

	public async Task End()
	{
		if (MatchEnded is not null)
			await MatchEnded.Invoke();
	}

	public Task Start()
	{
		return Task.CompletedTask;
	}

	public Task Update(HexCore.GameMatch.Match match)
	{

		return Task.CompletedTask;
	}
}

public struct RecordedAction {
	public required string PlayerName { get; set; }
	public required string Action { get; set; }
}

public class ActionAggregate {
	public List<RecordedAction> Actions { get; } = new();
}

public interface IActionDisplay {
	public void Load(RecordedAction action);
}

public partial class MatchRecording : Control
{
	#region Packed scenes

	[Export]
	private PackedScene ActionDisplayPS { get; set; }

	#endregion

	#region Nodes
	
	public Match.Match MatchNode { get; private set; }
	public Control OverlayNode { get; private set; }
	public Container ActionContainerNode { get; private set; }

	public HttpRequest FetchRecordRequestNode { get; private set; }
	public HttpRequest FetchConfigRequestNode { get; private set; }
	public HttpRequest FetchCardRequestNode { get; private set; }
	
	#endregion

	public string ApiUrl => GetNode<GlobalSettings>("/root/GlobalSettings").ApiUrl;
	
	public override void _Ready()
	{
		#region Node fetching
		
		MatchNode = GetNode<Match.Match>("%Match");
		OverlayNode = GetNode<Control>("%Overlay");
		ActionContainerNode = GetNode<Container>("%ActionContainer");
		
		FetchRecordRequestNode = GetNode<HttpRequest>("%FetchRecordRequest");
		FetchConfigRequestNode = GetNode<HttpRequest>("%FetchConfigRequest");
		FetchCardRequestNode = GetNode<HttpRequest>("%FetchCardRequest");
		
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
		var cm = new ApiCardMaster(ApiUrl, FetchCardRequestNode);
		
		var view = new RecordingMatchView();
		var match = new HexCore.GameMatch.Match("", config, cm, record.Seed) {
			// SystemLogger = new ConsoleLogger(),
			View = view,
		};

		// TODO change
		match.InitialSetup("../HexCore/core.lua");

		var aAggregate = new ActionAggregate();

		foreach (var p in record.Players) {
			var controller = new QueuedActionPlayerController(p, aAggregate);
			var deck = DeckTemplate.FromText(p.Deck);
			await match.AddPlayer(p.Name, deck, controller);
			GD.Print("added player");
		}

		_ = Task.Run(async () => await RunMatch(match, aAggregate));
	}

	private async Task RunMatch(HexCore.GameMatch.Match match, ActionAggregate aggregate) {
		try {
			await match.Start();

			GD.Print("match completed");
		} catch (Exception) {
			GD.Print("match crashed");
		}

		CallDeferred("LoadActionAggregate", new Wrapper<ActionAggregate>(aggregate));
	}

	private void LoadActionAggregate(Wrapper<ActionAggregate> aggregateW) {
		var aggregate = aggregateW.Value;

		while (ActionContainerNode.GetChildCount() > 0)
			ActionContainerNode.RemoveChild(ActionContainerNode.GetChild(0));

		foreach (var action in aggregate.Actions) {
			var child = ActionDisplayPS.Instantiate() as Control;
			GD.Print(child.CustomMinimumSize);
			ActionContainerNode.AddChild(child);

			var display = child as IActionDisplay;
			display.Load(action);
		}
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

