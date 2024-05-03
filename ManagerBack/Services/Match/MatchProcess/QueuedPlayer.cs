using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using HexCore.GameMatch.View;
using ManagerBack.Hubs;
using Shared;
using Utility;

namespace ManagerBack.Services;

/// <summary>
/// Queued player status
/// </summary>
public enum QueuedPlayerStatus {
    /// <summary>
    /// Waiting for player data
    /// </summary>
    WAITING_FOR_DATA,

    /// <summary>
    /// Ready to begin the match
    /// </summary>
    READY
}

/// <summary>
/// Match player info
/// </summary>
class PlayerInfo {
    /// <summary>
    /// Player name
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Text representation of the player deck
    /// </summary>
    public required string Deck { get; set; }

    /// <summary>
    /// The user's password
    /// </summary>
    public required string Password { get; set; }
}

/// <summary>
/// A player object that wants to participate in a match
/// </summary>
public class QueuedPlayer {
    /// <summary>
    /// Player information changed delegate
    /// </summary>
    public delegate Task PlayerChanged();

    /// <summary>
    /// Player information changed event
    /// </summary>
    public event PlayerChanged? Changed;
    
    /// <summary>
    /// Player status updated delegate
    /// </summary>
    public delegate Task StatusUpdate();

    /// <summary>
    /// Player status updated event 
    /// </summary>
    public event StatusUpdate? StatusUpdated;

    /// <summary>
    /// Player status
    /// </summary>
    private QueuedPlayerStatus _status = QueuedPlayerStatus.WAITING_FOR_DATA;

    /// <summary>
    /// Player status, fires updated events on change
    /// </summary>
    public QueuedPlayerStatus Status {
        get => _status;
        set {
            _status = value;
            StatusUpdated?.Invoke();
            Changed?.Invoke();
        }
    }

    /// <summary>
    /// Player controller
    /// </summary>
    [JsonIgnore]
    public IPlayerController Controller { get; }

    /// <summary>
    /// Player name
    /// </summary>
    private string? _name = null;

    /// <summary>
    /// Player name, fires Changed event on change
    /// </summary>
    public string? Name
    {
        get { return _name; }
        set { _name = value; Changed?.Invoke(); }
    }

    /// <summary>
    /// Text representation of the player's deck
    /// </summary>
    private string? _deck = null;

    /// <summary>
    /// Text representation of the player's deck, fires the Updated event on change
    /// </summary>
    public string? Deck
    {
        get { return _deck; }
        set { _deck = value; Changed?.Invoke(); }
    }

    /// <summary>
    /// Is bot flag
    /// </summary>
    public bool IsBot { get; }
    
    /// <summary>
    /// Connection checker
    /// </summary>
    [JsonIgnore]
    public IConnectionChecker Checker { get; }

    public QueuedPlayer(IPlayerController controller, IConnectionChecker checker, bool isBot)
    {
        IsBot = isBot;
        Controller = controller;
        Checker = checker;
    }

    /// <summary>
    /// Getter for the player's name
    /// </summary>
    /// <returns>Player name</returns>
    public string GetName() {
        return Name!;
    }

    /// <summary>
    /// Getter for the player's deck
    /// </summary>
    /// <returns>Deck template</returns>
    public DeckTemplate GetDeck() {
        return DeckTemplate.FromText(Deck!);
    }

    /// <summary>
    /// Reads and sets the player info
    /// </summary>
    /// <param name="match">Match process</param>
    /// <param name="checker">Connection checker</param>
    /// <returns>Error message if exception was thrown, else empty string</returns>
    public async Task<string> ReadPlayerInfo(MatchProcess match, IConnectionChecker checker) {
        try {
            var data = await checker.Read();
            var info = JsonSerializer.Deserialize<PlayerInfo>(data, Common.JSON_SERIALIZATION_OPTIONS);

            // * failed to read data, consider the connection to be invalid
            if (info is null) {
                return "failed to parse info";
            }

            if (!match.CheckPassword(info.Password)) return "incorrect password";

            Name = info.Name;
            Deck = info.Deck;
        } catch (Exception e) {
            System.Console.WriteLine(e.Message);
            System.Console.WriteLine(e.StackTrace);
            return e.Message;
        }
        return string.Empty;
    }
}
