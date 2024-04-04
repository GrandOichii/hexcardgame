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

public enum QueuedPlayerStatus {
    WAITING_FOR_DATA,
    READY
}

class PlayerInfo {
    public required string Name { get; set; }
    public required string Deck { get; set; }
}

public class QueuedPlayer {

    public delegate Task PlayerChanged();
    public event PlayerChanged? Changed;
    
    public delegate Task StatusUpdate();
    public event StatusUpdate? StatusUpdated;
    private QueuedPlayerStatus _status = QueuedPlayerStatus.WAITING_FOR_DATA;
    public QueuedPlayerStatus Status {
        get => _status;
        set {
            _status = value;
            StatusUpdated?.Invoke();
            Changed?.Invoke();
        }
    }

    [JsonIgnore]
    public IPlayerController Controller { get; }

    // public PlayerConfig Config { get; }
    private string? _name = null;
    public string? Name
    {
        get { return _name; }
        set { _name = value; Changed?.Invoke(); }
    }
    private string? _deck = null;
    public string? Deck
    {
        get { return _deck; }
        set { _deck = value; Changed?.Invoke(); }
    }
    
    public bool IsBot { get; }
    
    [JsonIgnore]
    public IConnectionChecker Checker { get; }

    public QueuedPlayer(IPlayerController controller, IConnectionChecker checker, bool isBot)
    {
        IsBot = isBot;
        Controller = controller;
        Checker = checker;
    }

    public string GetName() {
        return Name!;
    }

    public DeckTemplate GetDeck() {
        return DeckTemplate.FromText(Deck!);
    }

    public async Task<bool> ReadPlayerInfo(IConnectionChecker checker) {
        try {
            var data = await checker.Read();
            var info = JsonSerializer.Deserialize<PlayerInfo>(data, Common.JSON_SERIALIZATION_OPTIONS);

            // * failed to read data, consider the connection to be invalid
            if (info is null) {
                return false;
            }

            Name = info.Name;
            Deck = info.Deck;
        } catch (Exception e) {
            System.Console.WriteLine(e.Message);
            System.Console.WriteLine(e.StackTrace);
            return false;
        }
        return true;
    }
}