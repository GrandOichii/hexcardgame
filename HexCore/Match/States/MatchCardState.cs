using System.Text.Json.Serialization;

namespace HexCore.GameMatch.States;


// TODO add docs
public struct MatchCardState
{
    [JsonPropertyName("mid")]
    public string MID { get; set; }
    [JsonPropertyName("id")]
    public string ID { get; set; }
    public string OwnerID { get; set; }
    [JsonPropertyName("can")]
    public List<string> AvailableActions { get; set; }

    public string Name { get; set; }
    public string Type { get; set; }
    public long Cost { get; set; }
    public string Text { get; set; }
    public long Life { get; set; }
    public long Power { get; set; }
    public long Movement { get; set; }
    public bool HasDefence { get; set; }
    public long Defence { get; set; }

    public MatchCardState(ExpansionCard card) {
        MID = "";
        ID = card.GetCID();
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

    public MatchCardState(MatchCard card)
    {
        MID = card.MID;
        ID = card.Original.GetCID();
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

    static public List<MatchCardState> FromCardList(List<MatchCard> cards) {
        var result = new List<MatchCardState>();
        foreach (var card in cards)
            result.Add(new MatchCardState(card));
        return result;
    }
}
