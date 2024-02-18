using System.Reflection;
using Microsoft.AspNetCore.SignalR;

namespace ManagerBack.Hubs;
public sealed class MatchHub : Hub {
    private readonly IMatchService _matchServices;
    private readonly IHubContext<MatchHub> _hubContext;

    public MatchHub(IMatchService matchServices, IHubContext<MatchHub> hubContext)
    {
        _matchServices = matchServices;
        _hubContext = hubContext;
    }


    // public async Task Connect(string matchId) {
    //     System.Console.WriteLine("Connect to match " + matchId);
        
    //     // load cards
    //     var cm = new FileCardMaster();
    //     cm.LoadCardsFrom("../cards");

    //     // load decks
    //     var deckText = File.ReadAllText("../decks/deck1.deck");
    //     var deckTemplate = DeckTemplate.FromText(deckText);

    //     // load match config
    //     var configText = File.ReadAllText("../configs/normal.json");
    //     var config = MatchConfig.FromJson(configText);

    //     // match master
    //     var mCreator = MatchMaster.Instance;

    //     // create match
    //     var match = mCreator.New(cm, config);
    //     // match.StrictMode = false;
    //     match.AllowCommands = true;
    //     match.SystemLogger = new ConsoleLogger();
    //     Console.WriteLine("Waiting for connection...");

    //     // player controllers
    //     var p1Controller = new SignalRPlayerController(_hubContext, Context.UserIdentifier!, Context.ConnectionId);
        
    //     var p2Controller = new LuaPlayerController("../bots/basic.lua");

    //     // create players
    //     var p1 = new Player(match, "P1", deckTemplate, p1Controller);
    //     var p2 = new Player(match, "P2", deckTemplate, p2Controller);

    //     // start match
    //     RunMatch(match);
    //     // match.Start();
    // }

    // private async void RunMatch(Match m) {
    //     m.Start();
    // }
}


// /// <summary>
// /// Player controller, controlled by a SignalR connection
// /// </summary>
// public class SignalRPlayerController : IPlayerController {
//     private readonly string _connId;
//     private readonly string _userId;
//     private readonly IHubContext<MatchHub> _hubContext;


//     public SignalRPlayerController(IHubContext<MatchHub> hubContext, string userId, string connId)
//     {
//         _hubContext = hubContext;
//         _userId = userId;
//         _connId = connId;
//     }


//     private void Write(string message) {
//         _hubContext.Clients.Client(_connId).SendAsync("GameMethod", message).Wait();
//     }

//     private string Read() {
//         // TODO this doesn't work for some reason
//         // the solution seems to be to rework the entire game loop into async/await
//         var result = _hubContext.Clients.Client(_connId).InvokeAsync<string>("Respond", CancellationToken.None).GetAwaiter().GetResult();
//         return result;
//     }

//     // TODO
//     public string DoPromptAction(Player player, Match match)
//     {
//         var state = new MatchState(match, player, "action");

//         Write(state.ToJson());
        
//         return Read();
//     }

//     // TODO
//     public void Setup(Player player, Match match)
//     {
//         Write(new MatchInfoState(player, match).ToJson());
//     }

//     // TODO
//     public void Update(Player player, Match match)
//     {
//         Write(new MatchState(match, player, "update").ToJson());
//     }

//     // TODO
//     public void CleanUp()
//     {
//         // TODO
//     }

//     // TODO
//     public string DoPickTile(List<int[]> choices, Player player, Match match)
//     {
//         var request = "pt";
//         var args = new List<string>();
//         for (int i = 0; i < choices.Count; i++) {
//             args.Add("" + choices[i][0] + "." + choices[i][1]);
//         }
//         Write(new MatchState(match, player, request, args).ToJson());
        
//         return Read();
//     }

//     // TODO
//     public void SendCard(Match match, Player player, ExpansionCard card)
//     {
//         // var state = new MatchState(match, player, "card", new(){card.ToJson()});

//         // Write(state.ToJson());
//         Write(card.ToJson());
//     }

// }
