using System.Text.Json.Serialization;

namespace HexCore.GameMatch.States;


/// <summary>
/// State of the player
/// </summary>
public struct PlayerState {
    public string Name { get; set; }
    public string ID { get; set; }
    public int HandCount { get; set; }
    public int DeckCount { get; set; }
    public List<MatchCardState> Discard { get; set; }
    public int Energy { get; set; }

    public PlayerState(Player player) {
        Name = player.Name;
        ID = player.ID;
        HandCount = player.Hand.Cards.Count;
        DeckCount = player.Deck.Cards.Count;
        Energy = player.Energy;
        Discard = MatchCardState.FromCardList(player.Discard.Cards);
    }

}
