using System.Text.Json.Serialization;
using core.cards;

namespace core.match.states;


/// <summary>
/// State of the match card
/// </summary>
public class MCardState {
    [JsonPropertyName("mid")]
    public string MID { get; set; }
    [JsonPropertyName("id")]
    public string ID { get; set; }
    [JsonPropertyName("ownerID")]
    public string OwnerID { get; set; }
    [JsonPropertyName("can")]
    public List<string> AvaliableActions { get; set; }
    [JsonPropertyName("mod")]
    public Dictionary<string, object> Modifications { get; set; }

    public MCardState(MCard card) {
        MID = card.MID;
        ID = card.Original.CID;
        OwnerID = card.Owner.ID;
        
        // modifications
        Modifications = new();

        // name
        var cName = card.Name;
        if (cName != card.Original.Name)
            Modifications.Add("name", cName);
        var cType = card.Type;
        if (cType != card.Original.Type)
            Modifications.Add("type", cType);
        var cCost = card.Cost;
        if (cCost != card.Original.Cost)
            Modifications.Add("cost", cCost);
        // TODO life > power = movement

        // TODO available actions
    }
}