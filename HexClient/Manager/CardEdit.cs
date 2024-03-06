using Godot;
using HexCore.Cards;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;

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
	
	#region Signal connections
	
	private void OnSaveButtonPressed()
	{
		// TODO validate
		var oldName = _edited is not null ? $"{_edited.Expansion}::{_edited.Name}" : "";

		var result = new ExpansionCard {
			Name = NameEditNode.Text,
			Power = (int)PowerEditNode.Value,
			Life = (int)LifeEditNode.Value,
			Cost = (int)CostEditNode.Value,
			Type = TypeEditNode.Text,
			Text = TextEditNode.Text,
			Script = ScriptEditNode.Text,
			Expansion = ExpansionEditNode.Text,
		};

		EmitSignal(SignalName.Saved, new Wrapper<ExpansionCard>(result), oldName);
	}

	private void OnCancelButtonPressed()
	{
		// TODO confirm if there are any changes

		EmitSignal(SignalName.Closed);
	}
	
	#endregion
}


