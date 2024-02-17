using System.Text.Json.Serialization;

namespace Core.GameMatch.States;


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
