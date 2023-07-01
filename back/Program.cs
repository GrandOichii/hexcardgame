using core.cards;
using core.decks;
using core.players;
using core.match;
using util;
using Mindmagma.Curses;
using System.Text;
// using NCurses;


class QueuedActionsPlayerController : PlayerController
{
    public Queue<string> ActionQueue { get; } = new(new string[] {
        "play 5 4.1",
        "pass",
        "pass",
        "play 10 4.1",
    });

    public override string DoPromptAction(Player player, Match match)
    {
        return ActionQueue.Dequeue();
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
    static void Main(string[] args)
    {
        // load cards
        var cm = new FileCardMaster();
        cm.LoadCardsFrom("../cards");

        // load decks
        var deckText = File.ReadAllText("../decks/deck1.deck");
        var deckTemplate = DeckTemplate.FromText(deckText);

        // load match config
        var configText = File.ReadAllText("../configs/normal.json");
        var config = MatchConfig.FromJson(configText);

        // match master
        var mCreator = MatchMaster.Instance;

        // create match
        var match = mCreator.New(cm, config);
        match.AllowCommands = true;
        var view = new CursesMatchView();
        match.View = view;
        match.SystemLogger = new FileLogger("../recent_logs.txt");

        // player controllers
        var p1Controller = new CursesPlayerController(view);
        // var p1Controller = new QueuedActionsPlayerController();
        var p2Controller = p1Controller;

        // create players
        var p1 = new Player(match, "P1", deckTemplate, p1Controller);
        var p2 = new Player(match, "P2", deckTemplate, p2Controller);

        // start match
        match.Start();
    }
}