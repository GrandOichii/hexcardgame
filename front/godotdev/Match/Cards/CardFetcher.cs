using Godot;
using System;

using core.cards;

public partial class CardFetcher
{
	public FileCardMaster CMaster { get; }
	static public CardFetcher Instance { get; } = new();
	private CardFetcher() {
		CMaster = new();
		CMaster.LoadCardsFrom("../../cards");		
	}
	
	public Card Get(string cID) {
		return CMaster.Get(cID);
	}
}
