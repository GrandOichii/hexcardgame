using System.Net.Sockets;
using System.Text;
using core.cards;
using core.decks;
using core.match;
using core.match.states;
using NLua;
using Shared;
using util;
namespace core.players;

/// <summary>
/// Controller class, handles actions of player
/// </summary>
abstract public class PlayerController {
    /// <summary>
    /// Prompts the action from player
    /// </summary>
    /// <param name="player">Controller player</param>
    /// <param name="match">Match</param>
    /// <returns>Action of the player</returns>
    public string PromptAction(Player player, Match match) {
        var result = DoPromptAction(player, match);
        // TODO
        return result;
    }

    /// <summary>
    /// Prompts action and records it
    /// </summary>
    /// <param name="player">Controlled player</param>
    /// <param name="match">Match</param>
    /// <returns>The prompted action</returns>
    abstract public string DoPromptAction(Player player, Match match);

    /// <summary>
    /// Setups the player for the match
    /// </summary>
    /// <param name="player">Controller player</param>
    /// <param name="match">Match</param>
    abstract public void Setup(Player player, Match match);

    /// <summary>
    /// Updates the player abount the match
    /// </summary>
    /// <param name="player">Controlled player</param>
    /// <param name="match">Match</param>
    abstract public void Update(Player player, Match match);

    /// <summary>
    /// Cleans up the player data
    /// </summary>
    abstract public void CleanUp();

    /// <summary>
    /// Forces the controlled player to pick a tile and records it
    /// </summary>
    /// <param name="choices">List of tile coordinates. If empty, considers all tiles as valid choices</param>
    /// <param name="player">Controlled player</param>
    /// <param name="match">Match</param>
    /// <returns></returns>
    public string PickTile(List<int[]> choices, Player player, Match match) {
        var result = DoPickTile(choices, player, match);
        // TODO record
        // TODO? check for correctness
        return result;
    }

    /// <summary>
    /// Prompts the controlled player to pick a tile
    /// </summary>
    /// <param name="choices">List of tile coordinates. If empty, considers all tile as valid choices</param>
    /// <param name="player">Controlled player</param>
    /// <param name="match">Match</param>
    /// <returns></returns>
    abstract public string DoPickTile(List<int[]> choices, Player player, Match match);
}


/// <summary>
/// Player controller that always just passes it's turn
/// </summary>
public class InactivePlayerController : PlayerController
{
    public override void CleanUp()
    {
    }

    public override string DoPickTile(List<int[]> choices, Player player, Match match)
    {
        if (choices.Count == 0) {
            // TODO
        }
        return "" + choices[0][0] + "." + choices[1][0];
    }

    public override string DoPromptAction(Player player, Match match)
    {
        return "pass";
    }

    public override void Setup(Player player, Match match)
    {
    }

    public override void Update(Player player, Match match)
    {
    }
}


/// <summary>
/// Player controller, controlled by a TCP socket
/// </summary>
public class TCPPlayerController : PlayerController
{
    byte[] buffer = new byte[1024];
    private TcpClient _handler;

    public TCPPlayerController(TcpListener listener, Match match) {
        match.SystemLogger.Log("TCPPlayerController", "Waiting for connection");

        _handler = listener.AcceptTcpClient();
        
        match.SystemLogger.Log("TCPPlayerController", "Connection established, sending match info");
    }

    /// <summary>
    /// Writes the message to the socket
    /// </summary>
    /// <param name="message">Message</param>
    private void Write(string message) {
        var data = Encoding.UTF8.GetBytes(message);
        var handler = _handler.GetStream();
        NetUtil.Write(handler, message);
    }

    /// <summary>
    /// Reads a message from socket
    /// </summary>
    /// <returns>The read message</returns>
    private string Read() {
        // return Console.ReadLine();
        
        var stream = _handler.GetStream();
        var result = NetUtil.Read(stream);
        return result;
    }

    // public override void InformMatchEnd(Player controlledPlayer, Match match, bool won) {
    //     Write(MatchParsers.CreateMState(controlledPlayer, match, (won ? "won" : "lost"), new()).ToJson());
    // }

    // public override string ProcessPickAttackTarget(Player controlledPlayer, Match match, CardW card) {
    //     // TODO replace with available attacks
    //     var opponent = match.OpponentOf(controlledPlayer);
    //     var targets = new List<string>();
    //     foreach (var treasure in opponent.Treasures.Cards)
    //         targets.Add(treasure.GetCardWrapper().ID);

    //     Write(MatchParsers.CreateMState(controlledPlayer, match, "pick attack target", targets, "", card.ID).ToJson());
    //     return Read();
    // }

    public override string DoPromptAction(Player player, Match match)
    {
        var state = new MatchState(match, player, "action");

        Write(state.ToJson());
        
        return Read();
    }

    public override void Setup(Player player, Match match)
    {
        Write(new MatchInfoState(player, match).ToJson());
    }

    public override void Update(Player player, Match match)
    {
        Write(new MatchState(match, player, "update").ToJson());
    }

    public override void CleanUp()
    {
        // TODO
    }

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
}


/// <summary>
/// A player controller, whose actions are controlled by a lua script
/// </summary>
public class LuaPlayerController : PlayerController {
    static private readonly string SETUP_FNAME = "_Setup";
    static private readonly string PROMPT_ACTION_FNAME = "_PromptAction";
    static private readonly string UPDATE_FNAME = "_Update";
    static private readonly string CLEANUP_FNAME = "_Cleanup";

    private string _sPath;
    private Lua LState;

    private LuaFunction _setupF;
    private LuaFunction _promptActionF;
    private LuaFunction _updateF;
    private LuaFunction _cleaupF;
    public LuaPlayerController(string sPath) {
        _sPath = sPath;

        LState = new();
        LState.DoFile(_sPath);
        _setupF = LuaUtility.GetGlobalF(LState, SETUP_FNAME);
        _promptActionF = LuaUtility.GetGlobalF(LState, PROMPT_ACTION_FNAME);
        _updateF = LuaUtility.GetGlobalF(LState, UPDATE_FNAME);
        _cleaupF = LuaUtility.GetGlobalF(LState, CLEANUP_FNAME);
    }   

    public override string DoPromptAction(Player player, Match match)
    {        
        var result = _promptActionF.Call(new MatchState(match, player, "action").ToJson());
        return LuaUtility.GetReturnAs<string>(result);
    }

    public override void Setup(Player player, Match match)
    {

        _setupF.Call(new MatchState(match, player, "setup").ToJson());
    }

    public override void Update(Player player, Match match)
    {
        _updateF.Call(new MatchState(match, player, "update").ToJson());
    }

    public override void CleanUp() {
        _cleaupF.Call();
    }

    public override string DoPickTile(List<int[]> choices, Player player, Match match)
    {
        // TODO
        return "" + choices[0][0] + "." + choices[1][0];
        // throw new NotImplementedException();
    }
}

/// <summary>
/// In-match player object.
/// </summary>
public class Player {
    private Match _match;
    // remove
    public string Name { get; set; }
    public PlayerController Controller { get; }
    public string ID { get; }

    public int Energy { get; set; }
    private int _maxEnergy;
    public int MaxEnergy { get => _maxEnergy; set {
        _maxEnergy = value;
        var me =_match.Config.MaxEnergy;
        if (_maxEnergy > me && me != -1)
            _maxEnergy = me;
    } }

    public Dictionary<MCard, string> AllCards { get; }

    // zones
    public Zone<MCard> Deck { get; }
    public Zone<MCard> Discard { get; }
    public Zone<MCard> Hand { get; }

    // new log messages
    public List<List<MatchLogEntryPart>> NewLogs { get; set; }


    public Player(Match match, string name, DeckTemplate dTemplate, PlayerController controller) {
        NewLogs = new();
        _match = match;
        Name = name;
        Controller = controller;

        match.Players.Add(this);
        ID = match.PlayerIDCreator.Next();

        // zones
        Deck = dTemplate.ToDeck(match, this);
        Deck.Shuffle(match.Rnd);
        Hand = new();
        Discard = new();

        // all cards
        AllCards = new();
        foreach (var card in Deck.Cards)
            AllCards.Add(card, Zones.DECK);

        _match.SystemLogger.Log("PLAYER", "Added player " + name);
    }

    public void Setup() {
        Draw(_match.Config.StartingHandSize);

        Controller.Setup(this, _match);
    }

    /// <summary>
    /// Forces the player to draw an amount of cards
    /// </summary>
    /// <param name="amount">Amount of cards</param>
    public int Draw(int amount) {
        var cards = Deck.PopTop(amount);

        foreach (var card in cards)
            AllCards[card] = Zones.HAND;

        // _match.Emit("card_draw", new(){{"player", ToLuaTable(_match.LState)}, {"amount", amount}});

        Hand.AddToBack(cards);
        var message =  "Player " + ShortStr + " drew " + cards.Count + " card";
        if (cards.Count > 1)
            message += "s";
        _match.SystemLogger.Log("PLAYER", message + ".");

        message = Name + " drew " + cards.Count + " card";
        if (cards.Count > 1)
            message += "s";
        _match.Logger.ParseAndLog(message + ".");
        return cards.Count;
    }

    public string ShortStr => Name + " [" + ID + "]";

    // public LuaTable ToSmallLuaTable(Lua lState) {
    //     var result = LuaUtility.CreateTable(lState);

    //     return result;
    // }

    public bool TryPlayCard(MCard card) {
        var canPlay = card.CanBePlayed(this);
        if (!canPlay) return false;

        var payed = card.ExecCheckerFunc(MCard.PAY_COSTS_FNAME, card.Data, ID);
        if (!payed) {
            _match.SystemLogger.Log("WARN", "Player " + Name + " decided not to cast card " + card.ShortStr);
            return false;
        }

        return true;
    }
}