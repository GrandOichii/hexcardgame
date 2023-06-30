using System.Text.Json;
using System.Text.Json.Serialization;
using NLua;
using core.decks;
using core.players;
using util;
using core.scripts;
using core.tiles;
using core.cards;

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

    public Match(string id, MatchConfig config, CardMaster master) {
        CardMaster = master;
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
            player.Draw(Config.StartingHandSize);
        }

        View.Start();
        View.Update(this);
    }

    /// <summary>
    /// The main loop of the match
    /// </summary>
    public void Turns() {
        SystemLogger.Log("MATCH", "Started main match loop");
        // TODO
        while (Winner is null) {
            View.Update(this);
            var cPlayer = CurrentPlayer;
            foreach (var phase in _phases) {
                phase.Exec(this, cPlayer);

                if (!Active) break;
            }
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