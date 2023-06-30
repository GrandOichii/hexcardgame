using core.cards;
using core.decks;
using core.players;
using core.match;
using util;
using Mindmagma.Curses;
// using NCurses;

class TerminalPlayerController : PlayerController
{
    public override string DoPromptAction(Player player, Match match)
    {
        // NCurses.GetString()
        // return "p";
        return "aaa";
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
        var match = mCreator.New(config);
        match.View = new CursesMatchView();
        match.SystemLogger = new FileLogger("../recent_logs.txt");

        // player controllers
        var p1Controller = new TerminalPlayerController();
        var p2Controller = p1Controller;

        // create players
        var p1 = new Player(match, "P1", deckTemplate, p1Controller);
        var p2 = new Player(match, "P2", deckTemplate, p2Controller);

        // start match
        match.Start();
    }
}