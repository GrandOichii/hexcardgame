using System.Text.Json.Serialization;

namespace Core.GameMatch.States;


// TODO add docs
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
