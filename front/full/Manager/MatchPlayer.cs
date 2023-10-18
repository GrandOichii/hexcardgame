using Godot;
using System;
using System.Collections.Generic;

using core.decks;
using core.manager;

public partial class MatchPlayer : Control
{
	#region Nodes
	
	public CheckBox IsBotCheckNode { get; private set; }
	public LineEdit NameEditNode { get; private set; }
	public OptionButton DeckOptionNode { get; private set; }
	
	#endregion
	
	private Dictionary<string, DeckData> _deckIndex;
	
	public override void _Ready()
	{
		#region Node fetching
		
		IsBotCheckNode = GetNode<CheckBox>("%IsBotCheck");
		NameEditNode = GetNode<LineEdit>("%NameEdit");
		DeckOptionNode = GetNode<OptionButton>("%DeckOption");
		
		#endregion
	}
	
	public PlayerConfig Baked {
		get {
			var result = new PlayerConfig
			{
				IsBot = IsBotCheckNode.ButtonPressed,
				Name = NameEditNode.Text,
				DeckList = _deckIndex[DeckOptionNode.Text].ToDeckTemplate().ToText()
			};
			return result;
		}
	}
	
	public void UpdateDecks(List<DeckData> decks) {
		_deckIndex = new();
		DeckOptionNode.Clear();
		foreach (var deck in decks) {
			var name = deck.Name;
			_deckIndex.Add(name, deck);

			DeckOptionNode.AddItem(name);
		}
	}
}
