// TODO this is garbage
// the packages are not compatible, so I have to reimplement theese data classes

using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;
using core.decks;
using System;

public class CardData : core.cards.Card {
	[JsonPropertyName("expansions")]
	public List<ExpansionData> Expansions { get; set; }

	public override string ToJson() {
		return JsonSerializer.Serialize(this);
	}
}

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

/*
{
  "name": "string",
  "cards": [
    {
      "card": {
        "id": 0,
        "expansion": {
          "name": "string"
        },
        "card": {
          "name": "string",
          "cost": 0,
          "type": "string",
          "text": "string",
          "power": 0,
          "life": 0,
          "deckUsable": true,
          "script": "string",
          "expansions": [
            {
              "name": "string"
            }
          ]
        },
        "expansionNameKey": "string",
        "cardNameKey": "string"
      },
      "cardIDKey": 0
    }
  ]
}
*/

public class DeckCardData {

	[JsonPropertyName("expansionNameKey")]
	public string ExpansionNameKey { get; set; }
	
	[JsonPropertyName("cardNameKey")]
	public string CardNameKey { get; set; }

	[JsonPropertyName("deck")]
	public object? Deck { get; } = null;
	[JsonPropertyName("deckNameKey")]
	public string DeckNameKey { get; set; } = "";


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

	public string ToJson() {
		return JsonSerializer.Serialize(this);
	}
}
