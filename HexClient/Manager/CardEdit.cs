using Godot;
using HexCore.Cards;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Text.Json;

public partial class CardEdit : Control
{
	#region Signals
	
	[Signal]
	public delegate void ClosedEventHandler();
	[Signal]
	public delegate void SavedEventHandler(Wrapper<ExpansionCard> card, string oldName);
	
	#endregion
	
	#region Nodes
	
	public LineEdit NameEditNode { get; private set; }
	public LineEdit ExpansionEditNode { get; private set; }
	public SpinBox CostEditNode { get; private set; }
	public CheckBox DeckUsableCheckNode { get; private set; }
	public LineEdit TypeEditNode { get; private set; }
	public TextEdit TextEditNode { get; private set; }
	public TextEdit ScriptEditNode { get; private set; }
	public SpinBox PowerEditNode { get; private set; }
	public SpinBox LifeEditNode { get; private set; }

	public ConfirmationDialog ConfirmDiscardPopupNode { get; private set; }
	
	#endregion

	#nullable enable
	private ExpansionCard? _edited;
	#nullable disable
	
	public override void _Ready()
	{
		#region Node fetching

		NameEditNode = GetNode<LineEdit>("%NameEdit");
		ExpansionEditNode = GetNode<LineEdit>("%ExpansionEdit");
		CostEditNode = GetNode<SpinBox>("%CostEdit");
		DeckUsableCheckNode = GetNode<CheckBox>("%DeckUsableCheck");
		TypeEditNode = GetNode<LineEdit>("%TypeEdit");
		TextEditNode = GetNode<TextEdit>("%TextEdit");
		ScriptEditNode = GetNode<TextEdit>("%ScriptEdit");
		PowerEditNode = GetNode<SpinBox>("%PowerEdit");
		LifeEditNode = GetNode<SpinBox>("%LifeEdit");

		ConfirmDiscardPopupNode = GetNode<ConfirmationDialog>("%ConfirmDiscardPopup");
		
		#endregion
	}

	#nullable enable
	public void Load(ExpansionCard? card) {
	#nullable disable
		_edited = card;

		card ??= new() {
			Name = "",
			Cost = 1,
			Type = "",
			Text = "",
			Script = "",
			Expansion = ""
		};

		SetData(card);
	}

	private void SetData(ExpansionCard card) {
		NameEditNode.Text = card.Name;
		CostEditNode.Value = card.Cost;
		DeckUsableCheckNode.ButtonPressed = card.DeckUsable;
		TypeEditNode.Text = card.Type;
		TextEditNode.Text = card.Text;
		ScriptEditNode.Text = card.Script;
		PowerEditNode.Value = card.Power;
		LifeEditNode.Value = card.Life;
		ExpansionEditNode.Text = card.Expansion;
	}
	
	public ExpansionCard Baked {
		get => new()
		{
			Name = NameEditNode.Text,
			Power = (int)PowerEditNode.Value,
			Life = (int)LifeEditNode.Value,
			Cost = (int)CostEditNode.Value,
			Type = TypeEditNode.Text,
			Text = TextEditNode.Text,
			Script = ScriptEditNode.Text,
			Expansion = ExpansionEditNode.Text,
		};
	}

	private bool Compare() {
		var card = Baked;
		ExpansionCard oldCard = _edited;

		if (card.Name != oldCard.Name) return true;
		if (card.Cost != oldCard.Cost) return true;
		if (card.Expansion != oldCard.Expansion) return true;
		if (card.Expansion != oldCard.Expansion) return true;
		if (card.Type != oldCard.Type) return true;
		if (card.Text != oldCard.Text) return true;
		if (card.Power != oldCard.Power) return true;
		if (card.Life != oldCard.Life) return true;
		if (card.DeckUsable != oldCard.DeckUsable) return true;
		if (card.Script != oldCard.Script) return true;

		return false;
	}
	
	#region Signal connections
	
	private void OnSaveButtonPressed()
	{
		// * validation is currently done server-side, the window will not close unless user receives 200
		
		var oldName = _edited is not null ? $"{_edited.Expansion}::{_edited.Name}" : "";

		var result = Baked;

		EmitSignal(SignalName.Saved, new Wrapper<ExpansionCard>(result), oldName);
	}

	private void OnCancelButtonPressed()
	{
		var changesPresent = true;
		if (_edited is not null)
			changesPresent = Compare();
		if (changesPresent) {
			ConfirmDiscardPopupNode.Show();
			return;
		}

		EmitSignal(SignalName.Closed);
	}

	private void OnCopyToBufferButtonPressed()
	{
		var card = Baked;
		var data = JsonSerializer.Serialize(card);
		DisplayServer.ClipboardSet(data);
	}

	private void OnLoadFromBufferButtonPressed()
	{
		var data = DisplayServer.ClipboardGet();
		try {
			var card = JsonSerializer.Deserialize<ExpansionCard>(data);
			
			SetData(card);
		} catch {}
	}

	private void OnConfirmDiscardPopupConfirmed()
	{
		EmitSignal(SignalName.Closed);
	}
	
	#endregion
}





