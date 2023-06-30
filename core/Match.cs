using System.Text.Json;
using System.Text.Json.Serialization;
using core.decks;
using core.players;
using util;

namespace core.match;

/// <summary>
/// Configuraton object for match creation
/// </summary>
public struct MatchConfig {
    [JsonPropertyName("startingHandSize")]
    public int StartingHandSize { get; set; }
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
public class Match {
    public IDCreator PlayerIDCreator { get; set; } = new BasicIDCreator();
    public string ID { get; }
    public List<Player> Players { get; }
    public Player? Winner { get; set; } = null;
    private bool _allowCommands;

    public Match(string id, bool allowCommands = false) {
        ID = id;
        _allowCommands = allowCommands;
        Players = new();
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
    }

    /// <summary>
    /// The main loop of the match
    /// </summary>
    public void Turns() {
        // TODO
    }

    /// <summary>
    /// Cleans up after the match is finished
    /// </summary>
    public void CleanUp() {
        // TODO
    }
}


/// <summary>
/// Singleton object for creating and fetching matches
/// </summary>
public class MatchMaster {
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
    public Match New() {
        var id = IDCreator.Next();
        var result = new Match(id);
        Index.Add(id, result);
        return result;
    }
}