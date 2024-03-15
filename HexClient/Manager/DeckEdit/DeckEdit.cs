using Godot;
using HexCore.Decks;
using System;
using System.Collections.Generic;

namespace HexClient.Manager;

public interface IDeckEditCardDisplay {
	public void Load(string cid, int amount);
	public string GetCID();
	public int GetAmount();
}

public partial class DeckEdit : Control
{
	#region Packed scenes
	
	[Export]
	private PackedScene DeckEditCardDisplayPS { get; set; }
	
	#endregion
	
	#region Signals

	[Signal]
	public delegate void ClosedEventHandler();

	#endregion

	#region Nodes
	
	public LineEdit NameEditNode { get; private set; }
	public TextEdit DescriptionEditNode { get; private set; }
	public FlowContainer CardsContainerNode { get; private set; }
	
	#endregion

	private Deck? _edited;
	
	public override void _Ready()
	{
		#region Node fetching
		
		NameEditNode = GetNode<LineEdit>("%NameEdit");
		DescriptionEditNode = GetNode<TextEdit>("%DescriptionEdit");
		CardsContainerNode = GetNode<FlowContainer>("%CardsContainer");
		
		#endregion
	}

	public void Load(Deck? deck) {
		_edited = deck;

		var data = deck ?? new() {
			Id = "",
			Name = "",
			Description = "",
			Index = new()
		};

		SetData(data);
	}

	private void SetData(Deck deck) {
		NameEditNode.Text = deck.Name;
		DescriptionEditNode.Text = deck.Description;

		while (CardsContainerNode.GetChildCount() > 0)
			CardsContainerNode.RemoveChild(CardsContainerNode.GetChild(0));
		
		foreach (var pair in deck.Index) {
			var cid = pair.Key;
			var amount = pair.Value;

			var child = DeckEditCardDisplayPS.Instantiate();
			CardsContainerNode.AddChild(child);

			var display = child as IDeckEditCardDisplay;
			display.Load(cid, amount);
			
			// TODO connect signals
		}
	}

	public void TryClose() {
		// TODO

		EmitSignal(SignalName.Closed);
	}
	
	private Deck Baked {
		get {
			var index = new Dictionary<string, int>();
			foreach (var child in CardsContainerNode.GetChildren()) {
				if (child is not IDeckEditCardDisplay display) continue;

				var cid = display.GetCID();
				var amount = display.GetAmount();

				index.Add(cid, amount);
			}

			return new Deck {
				Id = "",
				Name = NameEditNode.Text,
				Description = DescriptionEditNode.Text,
				Index = index,
			};
		}
	}

	#region Signal connections

	private void OnCopyToBufferButtonPressed()
	{
		var deck = Baked.ToDeckTemplate();
		var data = deck.ToText();

		GD.Print(data);
		DisplayServer.ClipboardSet(data);
	}

	private void OnPasteFromBufferPressed()
	{
		var data = DisplayServer.ClipboardGet();
		try {
			var dt = DeckTemplate.FromText(data);
			var deck = Deck.FromDeckTemplate(dt);
			SetData(deck);
		} catch {}
	}

	private void OnCancelButtonPressed()
	{
		TryClose();
	}

	private void OnSaveButtonPressed()
	{
		// TODO
	}
	
	#endregion
}
