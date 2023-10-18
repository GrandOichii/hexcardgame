// TODO this is garbage
// the packages are not compatible, so I have to reimplement theese data classes

using System.Collections.Generic;
using System.Text.Json.Serialization;
using core.decks;

public class ExpansionData {
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class ExpansionCardData {
    [JsonPropertyName("card")]
    public core.cards.Card Card { get; set; }

    [JsonPropertyName("expansionNameKey")]
    public string Expansion { get; set; }
}

public class DeckCardData {
    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("card")]
    public ExpansionCardData Card { get; set; }

    [JsonIgnore]
    public string CID => Card.Expansion + "::" + Card.Card.Name;
}

public class DeckData {
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("cards")]
    public List<DeckCardData> Cards { get; set; }

    [JsonIgnore]
    public DeckCardData? this[string cid] {
        get {
            foreach (var card in Cards) {
                if (card.CID == cid)
                    return card;
            }
            return null;
        }
    }

    public void Remove(string cid) {
        var card = this[cid];
        Cards.Remove(card);
    }

    public DeckTemplate ToDeckTemplate() {
        var result = new DeckTemplate();

        // descriptors
        result.Descriptors.Add("name", Name);

        // cards
        result.Index = new();
        foreach (var card in Cards) {
            result.Index.Add(card.CID, card.Amount);
        }
        return result;
    }
}