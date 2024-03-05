using Godot;
using HexCore.Cards;
using System;

public partial class CardEdit : Control
{
	#region Nodes
	
	public LineEdit NameEdit { get; private set; }
	public SpinBox CostEdit { get; private set; }
	public CheckBox DeckUsableCheck { get; private set; }
	public LineEdit TypeEdit { get; private set; }
	public TextEdit TextEdit { get; private set; }
	public TextEdit ScriptEdit { get; private set; }
	public SpinBox PowerEdit { get; private set; }
	public SpinBox LifeEdit { get; private set; }
	
	#endregion
	
	public override void _Ready()
	{
		#region Node fetching

		NameEdit = GetNode<LineEdit>("%NameEdit");
		CostEdit = GetNode<SpinBox>("%CostEdit");
		DeckUsableCheck = GetNode<CheckBox>("%DeckUsableCheck");
		TypeEdit = GetNode<LineEdit>("%TypeEdit");
		TextEdit = GetNode<TextEdit>("%TextEdit");
		ScriptEdit = GetNode<TextEdit>("%ScriptEdit");
		PowerEdit = GetNode<SpinBox>("%PowerEdit");
		LifeEdit = GetNode<SpinBox>("%LifeEdit");
		
		#endregion
	}

	public void Load(ExpansionCard? card) {
		card ??= new() {
			Name = "",
			Cost = 1,
			Type = "",
			Text = "",
			Script = "",
			Expansion = ""
		};

		NameEdit.Text = card.Name;
		CostEdit.Value = card.Cost;
		DeckUsableCheck.ButtonPressed = card.DeckUsable;
		TypeEdit.Text = card.Type;
		TextEdit.Text = card.Text;
		ScriptEdit.Text = card.Script;
		PowerEdit.Value = card.Power;
		LifeEdit.Value = card.Life;
	}
}
