using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using DnsClient.Protocol;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.Serialization.Conventions;

namespace ManagerBack.Controllers;

[ApiController]
[Route("/api/v1/wstest")]
public class WebsocketTestController : ControllerBase {

    private readonly IMatchService _matchService;

    public WebsocketTestController(IMatchService matchService)
    {
        _matchService = matchService;
    }

    // TODO authorize
    // [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] MatchProcessConfig config) {
        // var userId = this.ExtractClaim(ClaimTypes.NameIdentifier);
        var userId = "";

        var match = await _matchService.Create(userId, config);
        return Ok(match.Id);
    }

    // TODO authorize
    // [Authorize]
    [HttpGet("connect/{matchId}")]
    public async Task Connect(string matchId) {
        if (HttpContext.WebSockets.IsWebSocketRequest) {
            // var userId = this.ExtractClaim(ClaimTypes.NameIdentifier);
            var userId = "";

            // ? are these status codes ok
            try {
                await _matchService.Connect(HttpContext.WebSockets, userId, matchId);
            } catch (MatchNotFoundException) {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
            } catch (MatchRefusedConnectionException) {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Locked;
            }
        } else {
            HttpContext.Response.StatusCode = 400;
        }
    }

    [HttpGet]
    public async Task<IActionResult> All() {
        return Ok(await _matchService.All());
    }

    private async Task RunMatch(WebSocket socket) {
        // load cards
        var cm = new FileCardMaster();
        cm.LoadCardsFrom("../cards");

        // load decks
        var deckText = System.IO.File.ReadAllText("../decks/deck1.deck");
        var deckTemplate = DeckTemplate.FromText(deckText);

        // load match config
        var configText = System.IO.File.ReadAllText("../configs/normal.json");
        var config = MatchConfig.FromJson(configText);

        // match master
        var mCreator = MatchMaster.Instance;

        // create match
        var match = mCreator.New(cm, config);
        // match.StrictMode = false;
        match.AllowCommands = true;
        // match.SystemLogger = new ConsoleLogger();
        Console.WriteLine("Waiting for connection...");

        // player controllers
        var p1Controller = new WebSocketPlayerController(socket);
        
        var p2Controller = new LuaPlayerController("../bots/basic.lua");

        // create players
        var p1 = new Player(match, "P1", deckTemplate, p1Controller);
        var p2 = new Player(match, "P2", deckTemplate, p2Controller);

        // start match
        match.Start();
    }

    private async Task Echo(WebSocket socket) {
        // await Task.Delay(100);
        var buffer = new byte[1024 * 4];
        var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        Console.WriteLine("Message received from Client");

        do {
            var serverMsg = Encoding.UTF8.GetBytes($"ping");
            await socket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
            Console.WriteLine("Message sent to Client");

            buffer = new byte[1024 * 4];
            result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            Console.WriteLine("Message received from Client");            
        }
        while (!result.CloseStatus.HasValue);
        await socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        Console.WriteLine("WebSocket connection closed");

        await RunMatch(socket);
    }

}
