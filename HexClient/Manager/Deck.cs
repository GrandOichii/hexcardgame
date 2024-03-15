
using System.Collections.Generic;
using HexCore.Decks;

namespace HexClient.Manager;

public struct Deck {
	public required string Id { get; set; }
	public required string Name { get; set; }
	public required string Description { get; set; }
	public required Dictionary<string, int> Index { get; set; }

	public DeckTemplate ToDeckTemplate() {
        var result = new DeckTemplate
        {
            Index = Index
        };

        // ? check the correct case
        result.SetDescriptor("name", Name);
		result.SetDescriptor("description", Description);

		return result;
	}

	public static Deck FromDeckTemplate(DeckTemplate deck) {
		return new Deck {
			Id = "",
			Name = deck.GetDescriptor("name"),
			Description = deck.GetDescriptor("description"),
			Index = deck.Index,
		};
	}
}