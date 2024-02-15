using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using DnsClient.Protocol;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.Serialization.Conventions;

namespace ManagerBack.Controllers;

// public class Global {
//     private static readonly int MAX_CONNECTIONS = 2;
//     private Global() {}
//     public static Global Instance { get; } = new();

//     public List<WebSocketConnection> Connections { get; set; } = new();
//     private int _cur = 0;
    
//     private async Task KeepAlive(WebSocket socket) {
//         while (socket.State == WebSocketState.Open) {
//             System.Console.WriteLine("Requested response");
//             await Write(socket, "ping");
//             var buf = new ArraySegment<byte>(new byte[1024]);
//             var ret = await socket.ReceiveAsync(buf, CancellationToken.None);

//             if (ret.MessageType == WebSocketMessageType.Close)
//             {
//                 break;
//             }                        
//             System.Console.WriteLine("Received response");
//         }
//         System.Console.WriteLine("REMOVING CONNECTION");
//         Connections.Remove(socket);
//         socket.Dispose();
//     } 

//     public async Task ConnectUser(WebSocket socket) {
//         Connections.Add(socket);
//         if (!CanConnect) {
//             System.Console.WriteLine("Started run");
//             Run();
//         }
//         // await KeepAlive(socket);
//     }

//     public bool CanConnect { get => Connections.Count < MAX_CONNECTIONS; }

//     public async Task Run() {
//         while (true) {
//             // Request message to the current user
//             await Write(Connections[_cur], "Enter the message");
//             var response = await Read(Connections[_cur]);

//             for (int i = 0; i < MAX_CONNECTIONS; ++i) {
//                 if (i == _cur) continue;

//                 await Write(Connections[i], response);
//             }
//         }
//     }

//     static private async Task<string> Read(WebSocket socket) {
//         var buffer = new byte[1024 * 4];
//         var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
//         // if (result.CloseStatus.HasValue) throw new Exception("connection closed");

//         return Encoding.UTF8.GetString(buffer);
//     }

//     static private async Task Write(WebSocket socket, string message) {
//         var serverMsg = Encoding.UTF8.GetBytes(message);

//         await socket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), WebSocketMessageType.Text, true, CancellationToken.None);
//     }
// }


[ApiController]
[Route("/api/v1/wstest")]
public class WebsocketTestController : ControllerBase {
    [HttpGet("connect")]
    public async Task Connect() {
        if (HttpContext.WebSockets.IsWebSocketRequest) {
            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            Console.WriteLine("Established ws connection!");
            await RunMatch(webSocket);
        } else {
            HttpContext.Response.StatusCode = 400;
        }
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
 
    }

}

/// <summary>
/// Player controller, controlled by a WebSocket connection
/// </summary>
public class WebSocketPlayerController : PlayerController {
    private readonly WebSocket _socket;

    public WebSocketPlayerController(WebSocket socket)
    {
        _socket = socket;
    }

    private void Write(string message) {
        var serverMsg = Encoding.UTF8.GetBytes(message);
        _socket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), WebSocketMessageType.Text, true, CancellationToken.None).Wait();
    }

    private string Read() {
        // TODO this doesn't work for some reason
        // the solution seems to be to rework the entire game loop into async/await
        var buffer = new byte[1024 * 4];
        _socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).Wait();
        var result = Encoding.UTF8.GetString(buffer).Replace("\0", string.Empty);

        return result;
    }

    // TODO
    public override string DoPromptAction(Player player, Match match)
    {
        var state = new MatchState(match, player, "action");

        Write(state.ToJson());
        
        return Read();
    }

    // TODO
    public override void Setup(Player player, Match match)
    {
        Write(new MatchInfoState(player, match).ToJson());
    }

    // TODO
    public override void Update(Player player, Match match)
    {
        Write(new MatchState(match, player, "update").ToJson());
    }

    // TODO
    public override void CleanUp()
    {
        // TODO
    }

    // TODO
    public override string DoPickTile(List<int[]> choices, Player player, Match match)
    {
        var request = "pt";
        var args = new List<string>();
        for (int i = 0; i < choices.Count; i++) {
            args.Add("" + choices[i][0] + "." + choices[i][1]);
        }
        Write(new MatchState(match, player, request, args).ToJson());
        
        return Read();
    }

    // TODO
    public override void SendCard(Match match, Player player, ExpansionCard card)
    {
        // var state = new MatchState(match, player, "card", new(){card.ToJson()});

        // Write(state.ToJson());
        Write(card.ToJson());
    }

}
