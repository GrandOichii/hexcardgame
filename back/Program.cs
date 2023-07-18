using core.cards;
using core.decks;
using core.players;
using core.match;
using util;
using Mindmagma.Curses;
using System.Text;
using System.Net;
using System.Net.Sockets;


class QueuedActionsPlayerController : PlayerController
{
    public Queue<string> ActionQueue { get; } = new( new string[] {} );

    public override string DoPromptAction(Player player, Match match)
    {
        return ActionQueue.Dequeue();
    }

    public override string DoPickTile(List<int[]> choices, Player player, Match match)
    {
        return ActionQueue.Dequeue();
    }

    public override void Setup(Player player, Match match)
    {
    }

    public override void Update(Player player, Match match)
    {
    }

    public override void CleanUp()
    {
    }
}

class CursesPlayerController : QueuedActionsPlayerController
{
    private CursesMatchView _view;
    public CursesPlayerController(CursesMatchView view) {
        _view = view;
    }

    public override string DoPromptAction(Player player, Match match)
    {
        if (ActionQueue.Count > 0) {
            return base.DoPromptAction(player, match);
        }
        
        NCurses.Echo();
        NCurses.SetCursor(1);
        NCurses.Move(0, 0);
        StringBuilder result = new();
        NCurses.GetString(result);
        NCurses.NoEcho();
        NCurses.SetCursor(0);
        return result.ToString();
    }
}


class Program {
    static private IPAddress ADDRESS = IPAddress.Any;
    static private int PORT = 9090;
    static private TcpListener listener = new TcpListener(new IPEndPoint(ADDRESS, PORT));


    static TCPPlayerController TCPPC(Match match) {
        return new TCPPlayerController(listener, match);
    }
    static string RunMatch() {
        // load cards
        var cm = new FileCardMaster();
        cm.LoadCardsFrom("../cards");

        // load decks
        var deckText = File.ReadAllText("../decks/generated.deck");
        var deckTemplate = DeckTemplate.FromText(deckText);

        // load match config
        var configText = File.ReadAllText("../configs/small.json");
        var config = MatchConfig.FromJson(configText);

        // match master
        var mCreator = MatchMaster.Instance;

        // create match
        var match = mCreator.New(cm, config);
        // match.StrictMode = false;
        match.AllowCommands = true;
        // var view = new CursesMatchView();
        // match.View = view;
        // match.SystemLogger = new FileLogger("../recent_logs.txt");
        // match.SystemLogger = new ConsoleLogger();

        // player controllers
        // var p1Controller = new CursesPlayerController(view);
        // var p1Controller = new QueuedActionsPlayerController();
        // var p1Controller = new InactivePlayerController();
        // var p1Controller = new LuaPlayerController("../bots/random.lua");
        var p1Controller = TCPPC(match);
        
        // var p2Controller = new InactivePlayerController();
        // var p2Controller = new LuaPlayerController("../bots/random.lua");
        var p2Controller = new LuaPlayerController("../bots/basic.lua");
        // var p2Controller = TCPPC(match);

        // create players
        var p1 = new Player(match, "P1", deckTemplate, p1Controller);
        var p2 = new Player(match, "P2", deckTemplate, p2Controller);

        // start match
        match.Start();

        return match.Winner.Name;
    }

    static void TrainBots() {

        listener.Start();
        Dictionary<string, int> result = new(){
            {"", 0},
            {"P1", 0},
            {"P2", 0},
        };
        
        // while (true) {
        //     try {

        // Task.WhenAny
        
        for (int i = 0; i < 300; i++) {
            Console.WriteLine("MATCH " + (i+1));
            // var tasks = new List<Task<string>>();
            // var task1 = Task.Run(() => RunMatch());
            // var task2 = Task.Run(() => WaitTask());
            // var completed = await Task.WhenAny(task1, task2);
            
            var winnerName = RunMatch();
            if (winnerName == "") Console.WriteLine("Ending due to timeout");
            result[winnerName]++;

            // copy data file as backup
            var file = "../bots/data.json";
            var tFile = "../bots/backups/" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".json";
            File.WriteAllBytes(tFile, File.ReadAllBytes(file)); 
        }
        foreach (var pair in result) {
            Console.WriteLine(pair.Key + " -> " + pair.Value);
        }
        //     } catch (Exception e){
        //         System.Console.WriteLine(e);
        //     }
        // }
        // MainAsync();
    }

    static void Main(string[] args)
    {
        listener.Start();
        // while (true) {
        //     try {
                RunMatch();
        //     } catch (Exception ex) {
        //         System.Console.WriteLine(ex);
        //     }
        // }
        // TrainBots();
    }
}