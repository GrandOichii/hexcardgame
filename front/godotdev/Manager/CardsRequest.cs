using Godot;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
//using Godot.Collections;

using core.cards;
using core.match.states;

public partial class CardWrapper : Resource {
	public MCardState Card { get; set; }
	public CardWrapper(MCardState card) {
		Card = card;
	}
}

public partial class CardsRequest : HttpRequest
{
	[Signal]
	public delegate void AddCardEventHandler(CardWrapper card);
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Request("http://localhost:5113/api/cards");
	}
	
	private void OnRequestCompleted(long result, long response_code, string[] headers, byte[] body)
	{
		var text = System.Text.Encoding.Default.GetString(body);
		var cards = JsonSerializer.Deserialize<List<Card>>(text);
		foreach (var card in cards)
			EmitSignal(SignalName.AddCard, new CardWrapper(new MCardState(card)));

	}
}


