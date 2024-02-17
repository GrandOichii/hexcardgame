using System.Text.Json.Serialization;

namespace Core.GameMatch.States;


// TODO add docs
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
    // [JsonPropertyName("text")]
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

    public MCardState(ExpansionCard card) {
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
        Text = card.Text;
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

        if (zone == ZoneTypes.DECK) return;
        // TODO? could change if allow to play cards from discard
        if (zone == ZoneTypes.DISCARD) return;
        if (zone == ZoneTypes.PLAYED) return;

        if (zone == ZoneTypes.HAND)
        {
            if (card.CanBePlayed(card.Owner))
                AvailableActions.Add("play");
        }
        if (zone == ZoneTypes.PLACED)
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
