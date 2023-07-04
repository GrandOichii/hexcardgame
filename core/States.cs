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
    public List<string> AvaliableActions { get; set; }

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
        if (card.IsUnit)
            Movement = card.Movement;

        AvaliableActions = new();
        if (card.Owner != card.Match.CurrentPlayer) return;

        var zone = card.Owner.AllCards[card];

        if (zone == Zones.DECK) return;
        // TODO? could change if allow to play cards from discard
        if (zone == Zones.DISCARD) return;
        if (zone == Zones.PLAYED) return;

        if (zone == Zones.HAND)
        {
            // TODO check for playability
            if (card.CanBePlayed(card.Owner))
                AvaliableActions.Add("play");
        }
        if (zone == Zones.PLACED)
        {
            if (card.IsUnit && card.CanMove)
                // TODO? specify directions
                AvaliableActions.Add("move");
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
/// State of the match card
/// </summary>
// public struct MCardState_Old {
//     [JsonPropertyName("mid")]
//     public string MID { get; set; }
//     [JsonPropertyName("id")]
//     public string ID { get; set; }
//     [JsonPropertyName("ownerID")]
//     public string OwnerID { get; set; }
//     [JsonPropertyName("can")]
//     public List<string> AvaliableActions { get; set; }
//     [JsonPropertyName("mod")]
//     public Dictionary<string, object> Modifications { get; set; }

//     public MCardState_Old(MCard card) {
//         MID = card.MID;
//         ID = card.Original.CID;
//         OwnerID = card.Owner.ID;
        
//         // modifications
//         Modifications = new();

//         // name
//         var cName = card.Name;
//         if (cName != card.Original.Name)
//             Modifications.Add("name", cName);
//         var cType = card.Type;
//         if (cType != card.Original.Type)
//             Modifications.Add("type", cType);
//         var cCost = card.Cost;
//         if (cCost != card.Original.Cost)
//             Modifications.Add("cost", cCost);
//         // TODO life > power = movement

//         var cLife = card.Life;
//         if (cLife != card.Original.Life)
//             Modifications.Add("life", cLife);

//         // if no life, can't be a Unit
//         if (cLife != -1) {
//             var cPower = card.Power;
//             if (cPower != card.Original.Power)
//                 Modifications.Add("power", cPower);
//             // if not unit, can't move
//             if (cPower != -1) {
//                 var cMovement = card.Movement;
//                 if (cMovement != card.MaxMovement)
//                     Modifications.Add("movement", cMovement);
//             }
//         }

//         AvaliableActions = new();
//         if (card.Owner != card.Match.CurrentPlayer) return;

//         var zone = card.Owner.AllCards[card];

//         if (zone == Zones.DECK) return;
//         // TODO? could change if allow to play cards from discard
//         if (zone == Zones.DISCARD) return;
//         if (zone == Zones.PLAYED) return;

//         if (zone == Zones.HAND) {
//             // TODO check for playability
//             if (card.CanBePlayed(card.Owner))
//                 AvaliableActions.Add("play");
//         }
//         if (zone == Zones.PLACED) {
//             if (card.IsUnit && card.CanMove)
//                 // TODO? specify directions
//                 AvaliableActions.Add("move");
//         }
//     }

//     static public List<MCardState> FromCardList(List<MCard> cards) {
//         var result = new List<MCardState>();
//         foreach (var card in cards)
//             result.Add(new MCardState(card));
//         return result;
//     }

//     public ModifiedCard WithModifications(Card original) {
//         var result = new ModifiedCard();
//         var card = new Card();
//         card.Text = original.Text;
//         // name
//         card.Name = original.Name;
//         if (Modifications.ContainsKey("name"))
//             card.Name = Modifications["name"].ToString();
        
//         // cost
//         card.Cost = original.Cost;
//         //if (Modifications.ContainsKey("cost"))
//         //    // TODO casting is bad
//         //    card.Cost = (Modifications["cost"] as JsonElement).GetInt32();

//         // type
//         card.Type = original.Type;
//         if (Modifications.ContainsKey("type"))
//             card.Type = Modifications["type"].ToString();

//         // life
//         card.Life = original.Life;
//         if (Modifications.ContainsKey("life"))
//             card.Life = ((JsonElement)Modifications["life"]).GetInt32();

//         // power
//         card.Power = original.Power;
//         if (Modifications.ContainsKey("power"))
//             card.Power = (int)Modifications["power"];

//         // movement
//         result.Movement = 1;
//         if (Modifications.ContainsKey("movement"))
//             result.Movement = ((JsonElement)Modifications["movement"]).GetInt32();
        
//         result.Card = card;    
//         return result;
//     }
// }


// public struct ModifiedCard {
//     public Card Card { get; set; }
//     public int Movement { get; set; }
// }


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
    [JsonPropertyName("myHand")]
    public List<MCardState> MyHand { get; set; }

    public MyDataState(Player player) {
        PlayerID = player.ID;
        MyHand = MCardState.FromCardList(player.Hand.Cards);
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

    public MatchState(Match match, Player player, string request) {
        NewLogs = player.NewLogs;
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