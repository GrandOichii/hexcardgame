using Godot;
using HexCore.Decks;
using System;
using System.Collections.Generic;

namespace HexClient.Manager;

public interface IDeckEditCardDisplay {
	public void Load(string cid, int amount);
	public string GetCID();
	public int GetAmount();
	public bool IsCardValid();
	public void SubcribeToAmountChanged(Action<int> action);
}

public partial class DeckEdit : Control
{
	#region Packed scenes
	
	[Export]
	private PackedScene DeckEditCardDisplayPS { get; set; }
	[Signal]
	public delegate void SavedEventHandler(Wrapper<Deck> card, string oldId);
	
	#endregion
	
	#region Signals

	[Signal]
	public delegate void ClosedEventHandler();

	#endregion

	#region Nodes
	
	public LineEdit NameEditNode { get; private set; }
	public TextEdit DescriptionEditNode { get; private set; }
	public FlowContainer CardsContainerNode { get; private set; }

	public AcceptDialog SaveErrorPopupNode { get; private set; }
	public AcceptDialog DiscardChangesPopupNode { get; private set; }
	
	#endregion

	private Deck? _edited;
	private bool _dataChanged = false;
	
	public override void _Ready()
	{
		#region Node fetching
		
		NameEditNode = GetNode<LineEdit>("%NameEdit");
		DescriptionEditNode = GetNode<TextEdit>("%DescriptionEdit");
		CardsContainerNode = GetNode<FlowContainer>("%CardsContainer");

		SaveErrorPopupNode = GetNode<AcceptDialog>("%SaveErrorPopup");
		DiscardChangesPopupNode = GetNode<AcceptDialog>("%DiscardChangesPopup");
		
		#endregion
	}

	public void Load(Deck? deck) {
		_edited = deck;
		_dataChanged = false;

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
			display.SubcribeToAmountChanged((_) => _dataChanged = true);
		}
	}

	public void TryClose() {
		if (_dataChanged) {
			DiscardChangesPopupNode.Show();
			return;
		}

		EmitSignal(SignalName.Closed);
	}
	
	private Deck Baked {
		get {
			var index = new Dictionary<string, int>();
			foreach (var child in CardsContainerNode.GetChildren()) {
				if (child is not IDeckEditCardDisplay display) continue;

				var cid = display.GetCID();
				if (!display.IsCardValid()) throw new Exception($"Card {cid} does not exist");

				var amount = display.GetAmount();
				if (amount <= 0) throw new Exception($"Card {cid} can't have amount {amount}");

				index.Add(cid, amount);
			}

			return new Deck {
				Id = _edited is not null ? _edited.Value.Id : "",
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
		var oldId = _edited is not null ? _edited.Value.Id : "";

		Deck result;
		try {
			result = Baked;
		} catch (Exception e) {
			// TODO? is catching general exceptions bad

			SaveErrorPopupNode.DialogText = e.Message;
			SaveErrorPopupNode.Show();

			return;
		}

		EmitSignal(SignalName.Saved, new Wrapper<Deck>(result), oldId);
	}

	private void OnDescriptionEditTextChanged()
	{
		_dataChanged = true;
	}

	private void OnNameEditTextChanged(string newText)
	{
		_dataChanged = true;
	}

	private void OnDiscardChangesPopupConfirmed()
	{
		EmitSignal(SignalName.Closed);
	}
	
	#endregion
}


