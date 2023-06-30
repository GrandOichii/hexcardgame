using System.Text.Json;
using System.Text.Json.Serialization;
using NLua;
using core.decks;
using core.players;
using util;
using core.scripts;
using core.tiles;

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
public struct MatchConfig
{
    [JsonPropertyName("startingHandSize")]
    public int StartingHandSize { get; set; }
    [JsonPropertyName("turnStartDraw")]
    public int TurnStartDraw { get; set; }
    [JsonPropertyName("setupScript")]
    public string SetupScript { get; set; }
    [JsonPropertyName("map")]
    public List<List<int>> Map { get; set; }

    /// <summary>
    /// Creates the match configuration from JSON
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    static public MatchConfig FromJson(string text) {
        var result = JsonSerializer.Deserialize<MatchConfig>(text);
        return result;
    }
}


/// <summary>
/// Match object, is used for playing matches
/// </summary>
public class Match
{
    static private List<MatchPhase> _phases = new(){
        new TurnStart(),
        new MainPhase(),
        new TurnEnd()
    };

    public IDCreator PlayerIDCreator { get; set; } = new BasicIDCreator();
    public string ID { get; }
    public List<Player> Players { get; }
    public Lua LState { get; }
    public ScriptMaster ScriptMaster { get; }
    public Map Map { get; }
    public Player? Winner { get; set; } = null;
    public bool Active => Winner is null;
    private bool _allowCommands;
    public Logger SystemLogger { get; set; } = new EmptyLogger();
    public MatchView View { get; set; } = new EmptyMatchView();

    public Match(string id, MatchConfig config, bool allowCommands = false) {
        ID = id;
        LState = new();
        ScriptMaster = new(this);
        _allowCommands = allowCommands;
        Players = new();

        Map = Map.FromConfig(this, config);
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
        // TODO

        View.Start();
    }

    /// <summary>
    /// The main loop of the match
    /// </summary>
    public void Turns() {
        SystemLogger.Log("MATCH", "Started main match loop");
        // TODO
        while (Winner is null) {
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
    public Match New(MatchConfig config) {
        var id = IDCreator.Next();
        var result = new Match(id, config);
        Index.Add(id, result);
        return result;
    }
}