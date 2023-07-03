using System.Text.Json;
using System.Text.Json.Serialization;
using NLua;
using core.decks;
using core.players;
using util;
using core.scripts;
using core.effects;
using core.tiles;
using core.cards;
using System.Text.RegularExpressions;

namespace core.match;

/// <summary>
/// View model of the match status
/// </summary>
abstract public class MatchView
{
    /// <summary>
    /// Start the view
    /// </summary>
    abstract public void Start();

    /// <summary>
    /// Updates the view with the new match data
    /// </summary>
    /// <param name="match">The displayed match</param>
    abstract public void Update(Match match);

    /// <summary>
    /// Ends the view
    /// </summary>
    abstract public void End();
}


/// <summary>
/// Empty match view, does nothing with the new information
/// </summary>
public class EmptyMatchView : MatchView
{
    public override void Start()
    {
    }

    public override void Update(Match match)
    {
    }
    public override void End()
    {
    }
}


/// <summary>
/// Configuraton object for match creation
/// </summary>
public class MatchConfig
{
    static Random _rnd = new Random();

    [JsonPropertyName("startingHandSize")]
    public int StartingHandSize { get; set; }
    [JsonPropertyName("turnStartDraw")]
    public int TurnStartDraw { get; set; }
    [JsonPropertyName("seed")]
    public int Seed { get; set; } = -1;
    [JsonPropertyName("setupScript")]
    public string SetupScript { get; set; }="error('NO SETUP SCRIPT')";
    [JsonPropertyName("map")]
    public List<List<int>> Map { get; set; }=new();

    /// <summary>
    /// Creates the match configuration from JSON
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    static public MatchConfig FromJson(string text) {
        var result = JsonSerializer.Deserialize<MatchConfig>(text);
        if (result is null) {
            throw new Exception("Failed to parse MatchConfig from " + text);
        }
        if (result.Seed == -1) {
            result.Seed = _rnd.Next();
        }
        return result;
    }
}


/// <summary>
/// Match object, is used for playing matches
/// </summary>
public class Match
{
    private static string CORE_FILE = "../core/core.lua";

    public Random Rnd { get; }
    static private List<MatchPhase> _phases = new(){
        new TurnStart(),
        new MainPhase(),
        new TurnEnd()
    };

    public IDCreator PlayerIDCreator { get; set; } = new BasicIDCreator();
    public IDCreator CardIDCreator { get; set; } = new BasicIDCreator();
    public CardMaster CardMaster { get; }
    public string ID { get; }
    public List<Player> Players { get; }
    public Lua LState { get; }
    public ScriptMaster ScriptMaster { get; }
    public Map Map { get; }
    public Player? Winner { get; set; } = null;
    public bool Active => Winner is null;
    public bool AllowCommands { get; set; }
    public Logger SystemLogger { get; set; } = new EmptyLogger();
    public MatchView View { get; set; } = new EmptyMatchView();
    public MatchConfig Config { get; }
    public MatchLogger Logger { get; }

    public Match(string id, MatchConfig config, CardMaster master) {
        CardMaster = master;
        Logger = new(this);
        ID = id;
        LState = new();
        ScriptMaster = new(this);
        Players = new();
        Config = config;
        Rnd = new Random(config.Seed);

        Map = Map.FromConfig(this, config);

        SystemLogger.Log("MATCH", "Running core file");
        LState.DoFile(CORE_FILE);
    }

    /// <summary>
    /// Checks whether can add a player with the specified name and deck
    /// </summary>
    /// <param name="name">Name of the player to be added</param>
    /// <param name="deck">Deck of the player to be added</param>
    /// <returns>A string with the reason why the player can't be added, otherwise returns empty string</returns>
    public string CanAddPlayer(string name, DeckTemplate deck) {
        foreach (var player in Players)
            if (player.Name == name)
                return "Player with name " + name + " is already joined.";
        // TODO check deck
        return "";
    }

    /// <summary>
    /// Starts the match
    /// </summary>
    public void Start() {
        Setup();
        Turns();
        CleanUp();
    }

    /// <summary>
    /// Sets up the match
    /// </summary>
    public void Setup() {
        SystemLogger.Log("MATCH", "Setting up match");
        
        SystemLogger.Log("MATCH", "Running map setup script");
        LState.DoString(Config.SetupScript);

        // setup players
        foreach (var player in Players) {
            player.Setup();
        }

        View.Start();
        View.Update(this);
    }

    /// <summary>
    /// The main loop of the match
    /// </summary>
    public void Turns() {
        SystemLogger.Log("MATCH", "Started main match loop");

        Logger.ParseAndLog("Match started");
        // TODO
        while (Winner is null) {
            View.Update(this);
            var cPlayer = CurrentPlayer;
            Logger.ParseAndLog(cPlayer.Name + " started their turn.");

            foreach (var phase in _phases) {
                phase.Exec(this, cPlayer);

                if (!Active) break;
            }
            Logger.ParseAndLog("Player " + cPlayer.Name + " passed their turn.");
            
            ++CurPlayerI;
        }
    }

    /// <summary>
    /// Cleans up after the match is finished
    /// </summary>
    public void CleanUp() {
        SystemLogger.Log("MATCH", "Performing cleanup");

        // TODO
        View.End();
    }

    /// <summary>
    /// Returns the current player
    /// </summary>
    /// <value>Current player</value>
    public Player CurrentPlayer => Players[CurPlayerI];
    private int _curPlayerI;
    public int CurPlayerI
    {
        get => _curPlayerI;
        set { 
            _curPlayerI = value; 
            if (_curPlayerI >= Players.Count)
                _curPlayerI = 0;
            if (_curPlayerI < 0)
                _curPlayerI = Players.Count - 1;
        }
    }
    
    /// <summary>
    /// Returns a player with the given ID
    /// </summary>
    /// <param name="pID">Player ID</param>
    /// <returns>Player</returns>
    public Player PlayerWithID(string pID) {
        foreach (var player in Players)
            if (player.ID == pID)
                return player;
        
        throw new Exception("Failed to find a player with ID " + pID);
    }

    /// <summary>
    /// Emits a singal to all cards
    /// </summary>
    /// <param name="signal">Signal string</param>
    /// <param name="args">Signal arguments</param>
    public void Emit(string signal, Dictionary<string, object> args) {
        var logMessage = "Emitted signal " + signal + ", args: ";
        foreach (var pair in args) logMessage += pair.Key + ":" + pair.Value.ToString() + " ";
        SystemLogger.Log("MATCH", logMessage);

        foreach (var player in Players) {
            var cards = player.AllCards;
            foreach (var pair in cards) {
                var zone = pair.Value;
                var card = pair.Key;
                var triggers = LuaUtility.TableGet<LuaTable>(card.Data, "triggers");
                foreach (var triggerO in triggers.Values) {
                    var triggerRaw = triggerO as LuaTable;
                    if (triggerRaw is null) throw new Exception("Trigger of card " + card.ShortStr + " is somehow value " + triggerO + " (not LuaTable)");
                    var z = LuaUtility.TableGet<string>(triggerRaw, "zone");
                    if (z != zone) continue;
                    var on = LuaUtility.TableGet<string>(triggerRaw, "on");
                    if (on != signal) continue;

                    var trigger = new Trigger(triggerRaw);
                    // TODO something else
                    SystemLogger.Log("MATCH", "Card " + card.ShortStr + " in zone " + zone + " of player " + player.ShortStr + " has a potential trigger");

                    var triggered = trigger.ExecCheck(LState, player, args);
                    if (!triggered) {
                        SystemLogger.Log("MATCH", "Card " + card.ShortStr + " in zone " + zone + " of player " + player.ShortStr + " failed to trigger");
                        continue;
                    }

                    var payed = trigger.ExecCosts(LState, player, args);
                    if (!payed) {
                        SystemLogger.Log("MATCH", "Player " + player.ShortStr + " did not pay cost of triggered ability of card " + card.ShortStr + " in zone " + zone);
                        continue;
                    }

                    SystemLogger.Log("MATCH", "Card " + card.ShortStr + " in zone " + zone + " of player " + player.ShortStr + " triggers");
                    trigger.ExecEffect(LState, player, args);
                }
            }
        }

        SystemLogger.Log("MATCH", "Finished emitting " + signal);
    }

    /// <summary>
    /// Checks all Units and Structures for having 0 life
    /// </summary>
    public void CheckZeroLife() {
        for (int i = 0; i < Map.Height; i++) {
            for (int j = 0; j < Map.Width; j++) {
                var tile = Map.Tiles[i, j];
                if (tile is null) continue;
                var en = tile.Entity;
                if (en is null) continue;
                if (!en.IsPlaceable) continue;
                if (en.Life != 0) continue;
                
                tile.Entity = null;
                if (!en.GoesToDiscard) continue;
                
                en.Owner.Discard.AddToBack(en);
                en.Owner.AllCards[en] = Zones.DISCARD;

                if (en.IsUnit) tile.HasGrave = true;
            }
        }
    }

    /// <summary>
    /// Returns the card with the specified match ID
    /// </summary>
    /// <param name="mID">Card match ID</param>
    /// <returns>Match card</returns>
    public MCard GetCard(string mID) {
        foreach (var player in Players)
            foreach (var card in player.AllCards.Keys)
                if (card.MID == mID)
                    return card;

        throw new Exception("Failed to get owner of the card with match ID " + mID);
    }

    /// <summary>
    /// Updates all of the current player's opponents
    /// </summary>
    public void UpdateOpponents() {
        var cur = CurrentPlayer;
        foreach (var player in Players)
            if (player != cur)
                player.Controller.Update(player, this);
    }
}


/// <summary>
/// Singleton object for creating and fetching matches
/// </summary>
public class MatchMaster
{
    static public MatchMaster Instance { get; } = new MatchMaster();
    static public IDCreator IDCreator { get; set; } = new BasicIDCreator();


    public Dictionary<string, Match> Index { get; set; }
    private MatchMaster() {
        Index = new();
    }

    /// <summary>
    /// Creates and saves a new Match object
    /// </summary>
    /// <returns>The created match</returns>
    public Match New(CardMaster cMaster, MatchConfig config) {
        var id = IDCreator.Next();
        var result = new Match(id, config, cMaster);
        Index.Add(id, result);
        return result;
    }
}


/// <summary>
/// Part of the message in the public log
/// </summary>
public struct MatchLogEntryPart {
    [JsonPropertyName("text")]
    public string Text { get; set; }
    [JsonPropertyName("cardRef")]
    public string CardRef { get; set; }

    public MatchLogEntryPart(string message, string cardRef) {
        Text = message;
        CardRef = cardRef;
    }
}


/// <summary>
/// Class for logging public messages in match
/// </summary>
public class MatchLogger {
    static public Regex CARD_NAME_MATCHER = new Regex("\\[\\[(.+)#(.+)\\]\\]");

    public List<List<MatchLogEntryPart>> Messages { get; }
    private Match _match;

    public MatchLogger(Match match) {
        _match = match;
        Messages = new();
    }

    public void Log(List<MatchLogEntryPart> message) {
        Messages.Add(message);

        foreach (var player in _match.Players)
            player.NewLogs.Add(message);
    }

    public void ParseAndLog(string message) {
        var groups = CARD_NAME_MATCHER.Split(message);
        var result = new List<MatchLogEntryPart>();
        // every odd is a match
        // TODO fix with new regex pattern:
        // 0 - normal message
        // 1 - card label
        // 2 - actual card name
        var lastMessage = "";
        for (int i = 0; i < groups.Length; i++) {
            var g = groups[i];
            if (g == "") continue;

            var a = i % 3;
            if (a == 0) {
                result.Add(new MatchLogEntryPart(g, ""));
                continue;
            }
            if (a == 1) {
                lastMessage = g;
                continue;
            }

            result.Add(new MatchLogEntryPart(lastMessage, g));
        }
        Log(result);
    }
}