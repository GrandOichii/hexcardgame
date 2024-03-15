using Godot;
using System;

namespace HexClient.Manager;

public interface IDeckEditCardDisplay {
	public void Load(string cid, int amount);
	// TODO
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

	#region Signal connections

	private void OnCopyToBufferButtonPressed()
	{
		// TODO
	}

	private void OnPasteFromBufferPressed()
	{
		// TODO
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
