using System.Text.Json.Serialization;

namespace HexCore.GameMatch.States;


// TODO add docs
public struct MyDataState {
    public string PlayerID { get; set; }
    public List<MatchCardState> Hand { get; set; }

    public MyDataState(Player player) {
        PlayerID = player.ID;
        Hand = MatchCardState.FromCardList(player.Hand.Cards);
    }
}
