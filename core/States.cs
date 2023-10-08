using System.Text.Json;
using System.Text.Json.Serialization;
using core.cards;
using core.players;
using core.tiles;

namespace core.match.states;


/// <summary>
/// Match info state, used for sending to players for UI setup
/// </summary>
public struct MatchInfoState {
    [JsonPropertyName("myID")]
    public string MyID { get; set; }
    [JsonPropertyName("myI")]
    public int MyI { get; set; }
    [JsonPropertyName("playerCount")]
    public int PlayerCount { get; set; }

    public MatchInfoState(Player player, Match match) {
        MyID = player.ID;
        MyI = match.Players.IndexOf(player);
        PlayerCount = match.Players.Count;
    }

    /// <summary>
    /// Converts the match info to JSON
    /// </summary>
    /// <returns>JSON string</returns>    
    public string ToJson() {
        var result = JsonSerializer.Serialize(this);
        if (result is null) throw new Exception("Failed to serialize match");
        return result;
    }

    /// <summary>
    /// Parses JSON to a MatchInfoState object
    /// </summary>
    /// <param name="json">JSON text</param>
    /// <returns>Parsed object</returns>
    static public MatchInfoState FromJson(string json) {
        var result = JsonSerializer.Deserialize<MatchInfoState>(json);
        return result;
    }
}

public struct MCardState
{
    [JsonPropertyName("mid")]
    public string MID { get; set; }
    [JsonPropertyName("id")]
    public string ID { get; set; }
    [JsonPropertyName("ownerID")]
    public string OwnerID { get; set; }
    [JsonPropertyName("can")]
    public List<string> AvailableActions { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("cost")]
    public long Cost { get; set; }
    [JsonPropertyName("text")]
    public string Text { get; set; }
    [JsonPropertyName("life")]
    public long Life { get; set; }
    [JsonPropertyName("power")]
    public long Power { get; set; }
    [JsonPropertyName("movement")]
    public long Movement { get; set; }
    [JsonPropertyName("hasDefence")]
    public bool HasDefence { get; set; }
    [JsonPropertyName("defence")]
    public long Defence { get; set; }

    public MCardState(Card card) {
        MID = "";
        ID = card.CID;
        OwnerID = "";
        AvailableActions = new();
        Name = card.Name;
        Type = card.Type;
        Cost = card.Cost;
        Text = card.Text;
        Life = card.Life;
        Power = card.Power;
        Movement = 0;

        HasDefence = false;
        Defence = 0;
    }

    public MCardState(MCard card)
    {
        MID = card.MID;
        ID = card.Original.CID;
        OwnerID = "";
        if (card.Owner is not null)
            OwnerID = card.Owner.ID;

        Name = card.Name;
        Type = card.Type;
        Text = card.Original.Text;
        Cost = card.Cost;
        Life = card.Life;
        Power = card.Power;
        Movement = -1;
        HasDefence = false;
        Defence = 0;
        if (card.IsPlaceable) {
            HasDefence = card.MaxDefence > 0;
            Defence = card.Defence;
        }
        if (card.IsUnit)
            Movement = card.Movement;

        AvailableActions = new();
        if (card.Owner != card.Match.CurrentPlayer) return;

        var zone = card.Owner.AllCards[card];

        if (zone == Zones.DECK) return;
        // TODO? could change if allow to play cards from discard
        if (zone == Zones.DISCARD) return;
        if (zone == Zones.PLAYED) return;

        if (zone == Zones.HAND)
        {
            if (card.CanBePlayed(card.Owner))
                AvailableActions.Add("play");
        }
        if (zone == Zones.PLACED)
        {
            if (card.IsUnit && card.CanMove)
                // TODO? specify directions
                AvailableActions.Add("move");
        }
    }

    static public List<MCardState> FromCardList(List<MCard> cards) {
        var result = new List<MCardState>();
        foreach (var card in cards)
            result.Add(new MCardState(card));
        return result;
    }
}


/// <summary>
/// State of the player
/// </summary>
public struct PlayerState {
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("id")]
    public string ID { get; set; }
    [JsonPropertyName("handCount")]
    public int HandCount { get; set; }
    [JsonPropertyName("deckCount")]
    public int DeckCount { get; set; }
    [JsonPropertyName("discard")]
    public List<MCardState> Discard { get; set; }
    [JsonPropertyName("energy")]
    public int Energy { get; set; }

    public PlayerState(Player player) {
        Name = player.Name;
        ID = player.ID;
        HandCount = player.Hand.Cards.Count;
        DeckCount = player.Deck.Cards.Count;
        Energy = player.Energy;
        Discard = MCardState.FromCardList(player.Discard.Cards);
    }

}


/// <summary>
/// State of the map
/// </summary>
public struct MapState {
    [JsonPropertyName("tiles")]
    public List<List<TileState?>> Tiles { get; set; }

    public MapState(Map map) {
        Tiles = new ();
        for (int i = 0; i < map.Height; i++) {
            var l = new List<TileState?>();
            for (int j = 0; j < map.Width; j++) {
                var tile = map.Tiles[i, j];
                if (tile is null) {
                    l.Add(null);
                    continue;
                }

                l.Add(new TileState(tile));
            }
            Tiles.Add(l);
        }
    }
}


/// <summary>
/// State of a tile
/// </summary>
public struct TileState {
    [JsonPropertyName("ownerID")]
    public string OwnerID { get; set; }
    [JsonPropertyName("hasGrave")]
    public bool HasGrave { get; set; }
    [JsonPropertyName("entity")]
    public MCardState? Entity { get; set; }
    public TileState(Tile tile) {
        OwnerID = "";
        if (tile.Owner is not null) OwnerID = tile.Owner.ID;

        HasGrave = tile.HasGrave;

        Entity = null;
        if (tile.Entity is not null) Entity = new MCardState(tile.Entity);
    }
}


public struct MyDataState {
    [JsonPropertyName("id")]
    public string PlayerID { get; set; }
    [JsonPropertyName("hand")]
    public List<MCardState> Hand { get; set; }

    public MyDataState(Player player) {
        PlayerID = player.ID;
        Hand = MCardState.FromCardList(player.Hand.Cards);
    }
}

/// <summary>
/// State of the match
/// </summary>
public struct MatchState {
    [JsonPropertyName("newLogs")]
    public List<List<MatchLogEntryPart>> NewLogs { get; set; }
    [JsonPropertyName("request")]
    public string Request { get; set; }
    [JsonPropertyName("players")]
    public List<PlayerState> Players { get; set; }
    [JsonPropertyName("map")]
    public MapState Map { get; set; }
    [JsonPropertyName("myData")]
    public MyDataState MyData { get; set; }
    [JsonPropertyName("curPlayerID")]
    public string CurPlayerID { get; set; }
    [JsonPropertyName("args")]
    public List<string> Args { get; set; }

    public MatchState(Match match, Player player, string request, List<string>? args=null) {
        if (args == null) args = new();

        NewLogs = player.NewLogs;
        Args = args;
        player.NewLogs = new();

        CurPlayerID = match.CurrentPlayer.ID;
        Request = request;

        MyData = new MyDataState(player);
        
        Players = new();
        foreach (var p in match.Players)
            Players.Add(new PlayerState(p));
        
        Map = new MapState(match.Map);
    }

    /// <summary>
    /// Converts the match state to JSON
    /// </summary>
    /// <returns>JSON string</returns>
    public string ToJson() {
        var result = JsonSerializer.Serialize(this);
        if (result is null) throw new Exception("Failed to serialize match");
        return result;
    }
    
    static public MatchState FromJson(string json) {
        var result = JsonSerializer.Deserialize<MatchState>(json);
        return result;
    }
}