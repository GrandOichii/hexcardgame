﻿using Mindmagma.Curses;
using System.Text;
using System.Net;
using System.Net.Sockets;
using HexCore.GameMatch.Players;
using HexCore.GameMatch;
using HexCore.Cards.Masters;
using HexCore.Decks;
using Microsoft.Extensions.Logging;
using HexCore.GameMatch.Players.Controllers;
using System.Text.Json;




// class CursesPlayerController : QueuedActionsPlayerController
// {
//     private CursesMatchView _view;
//     public CursesPlayerController(CursesMatchView view) {
//         _view = view;
//     }

//     public override string DoPromptAction(Player player, Match match)
//     {
//         if (ActionQueue.Count > 0) {
//             return base.DoPromptAction(player, match);
//         }

//         NCurses.Echo();
//         NCurses.SetCursor(1);
//         NCurses.Move(0, 0);
//         StringBuilder result = new();
//         NCurses.GetString(result);
//         NCurses.NoEcho();
//         NCurses.SetCursor(0);
//         return result.ToString();
//     }
// }

class Program {
    // static private IPAddress ADDRESS = IPAddress.Any;
    // static private int PORT = 9090;
    // static private TcpListener listener = new(new IPEndPoint(ADDRESS, PORT));


    // static TCPPlayerController TCPPC(Match match) {
    //     return new TCPPlayerController(listener, match);
    // }
    // static async Task<string> RunMatch() {
    //     // load cards
    //     var cm = new FileCardMaster();
    //     cm.LoadCardsFrom("../cards");

    //     // load decks
    //     var deckText = File.ReadAllText("../decks/deck1.deck");
    //     var deckTemplate = DeckTemplate.FromText(deckText);

    //     // load match config
    //     var configText = File.ReadAllText("../configs/normal.json");
    //     var config = MatchConfig.FromJson(configText);


    //     // create match
    //     var match = new Match("1", config, cm)
    //     {
    //         // match.StrictMode = false;
    //         AllowCommands = true,
    //         // var view = new CursesMatchView();
    //         // match.View = view;
    //         // match.SystemLogger = new FileLogger("../recent_logs.txt");
    //         SystemLogger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("Match")
    //     };

    //     match.InitialSetup("../HexCore/core.lua");
        
    //     // Console.WriteLine("Waiting for connection...");

    //     // player controllers
    //     // var p1Controller = new CursesPlayerController(view);
    //     // var p1Controller = new QueuedActionsPlayerController();
    //     // var p1Controller = new InactivePlayerController();
    //     var p1Controller = new LuaPlayerController("../bots/random.lua");
    //     // var p1Controller = TCPPC(match);
        
    //     // var p2Controller = new InactivePlayerController();
    //     // var p2Controller = new LuaPlayerController("../bots/random.lua");
    //     var p2Controller = new LuaPlayerController("../bots/basic.lua");
    //     // var p2Controller = TCPPC(match);


    //     // create players
    //     await match.AddPlayer("P1", deckTemplate, p1Controller);
    //     await match.AddPlayer("P2", deckTemplate, p1Controller);

    //     // start match
    //     await match.Start();

    //     return match.Winner!.Name;
    // }

    // static void TrainBots() {

    //     listener.Start();
    //     Dictionary<string, int> result = new(){
    //         {"", 0},
    //         {"P1", 0},
    //         {"P2", 0},
    //     };
        
    //     // while (true) {
    //     //     try {

    //     // Task.WhenAny
        
    //     for (int i = 0; i < 300; i++) {
    //         try {
    //             Console.WriteLine("MATCH " + (i+1));
    //             // var tasks = new List<Task<string>>();
    //             // var task1 = Task.Run(() => RunMatch());
    //             // var task2 = Task.Run(() => WaitTask());
    //             // var completed = await Task.WhenAny(task1, task2);
                
    //             var winnerName = RunMatch();
    //             if (winnerName == "") Console.WriteLine("Ending due to timeout");
    //             result[winnerName]++;

    //             // copy data file as backup
    //             var file = "../bots/data.json";
    //             var tFile = "../bots/backups/" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".json";
    //             File.WriteAllBytes(tFile, File.ReadAllBytes(file)); 
    //         } catch (Exception e) {
    //             Console.WriteLine("Threw exception: " + e);
    //         }
    //     }
    //     foreach (var pair in result) {
    //         Console.WriteLine(pair.Key + " -> " + pair.Value);
    //     }
    //     //     } catch (Exception e){
    //     //         System.Console.WriteLine(e);
    //     //     }
    //     // }
    //     // MainAsync();
    // }

    static void Main(string[] args)
    {
        var deck = DeckTemplate.FromText("dev::Elven Outcast#3;name=deck2|description=");
        System.Console.WriteLine(deck.GetDescriptor("name"));
        System.Console.WriteLine(deck.GetDescriptor("description"));
        // listener.Start();
        // while (true) {
        //     try {
        //         RunMatch().Wait();
        //     } catch (Exception e) {
        //         System.Console.WriteLine(e);
        //     }
        //     break;
        // }
        // TrainBots();
    }
}