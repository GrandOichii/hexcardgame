using Godot;
using System.Collections.Generic;
using System.Linq;


// using HexCore.Cards;
using System.Text.Json;

namespace HexClient.Manager;

public partial class CardEdit : Control
{
	private readonly static Dictionary<string, List<string>> TYPE_MAP = new(){
		{ "Spell", new() },
		{ "Structure", new() },
		{ "Unit", new() {
			"Warrior",
			"Rogue",
			"Mage"
		} },
	};
	
	#region Signals
	
	[Signal]
	public delegate void ClosedEventHandler();
	[Signal]
	public delegate void SavedEventHandler(Wrapper<HexCore.Cards.ExpansionCard> card, string oldName);
	
	#endregion
	
	#region Nodes
	
	public LineEdit NameEditNode { get; private set; }
	public LineEdit ExpansionEditNode { get; private set; }
	public SpinBox CostEditNode { get; private set; }
	public CheckBox DeckUsableCheckNode { get; private set; }
	public TextEdit TextEditNode { get; private set; }
	public CodeEdit ScriptEditNode { get; private set; }
	public SpinBox PowerEditNode { get; private set; }
	public SpinBox LifeEditNode { get; private set; }
	public Card PreviewCardNode { get; private set; }
	public Container TypeChecksContainerNode { get; private set; }
	public Container SubtypeChecksContainerNode { get; private set; }

	public ConfirmationDialog ConfirmDiscardPopupNode { get; private set; }
	public Window PreviewWindowNode { get; private set; }
	
	#endregion

	private readonly List<CheckBox> _typeCheckBoxes = new();
	private readonly List<CheckBox> _subtypeCheckBoxes = new();

	#nullable enable
	private HexCore.Cards.ExpansionCard? _edited;
	#nullable disable

	public string CardType { 
		get {
			var result = "";
			foreach (var check in _typeCheckBoxes) {
				if (!check.ButtonPressed) continue;
				var type = check.GetMeta("Value").AsString();
				result += type;
			}
			var sub = new List<string>();
			foreach (var check in _subtypeCheckBoxes) {
				if (check.Visible && check.ButtonPressed) {
					sub.Add(check.GetMeta("Value").AsString());
				}
			}
			if (sub.Count > 0)
				result += " - " + string.Join(" ", sub.ToArray());
			
			return result;
		}
		set {
			var split = value.Split(" - ");
			var type = split[0];
			foreach (var check in _typeCheckBoxes) {
				check.ButtonPressed = type == check.GetMeta("Value").AsString();
			}

			if (split.Length == 1) return;
			var validSubtypes = TYPE_MAP[type];
			var subtypes = split[1].Split(" ");
			foreach (var check in _subtypeCheckBoxes) {
				var subtype = check.GetMeta("Value").AsString();

				check.Visible = false;
				if (!validSubtypes.Contains(subtype)) return;

				check.Visible = true;
				check.ButtonPressed = subtypes.Contains(subtype);
			}
		} 
	}
	
	public override void _Ready()
	{
		#region Node fetching

		NameEditNode = GetNode<LineEdit>("%NameEdit");
		ExpansionEditNode = GetNode<LineEdit>("%ExpansionEdit");
		CostEditNode = GetNode<SpinBox>("%CostEdit");
		DeckUsableCheckNode = GetNode<CheckBox>("%DeckUsableCheck");
		TextEditNode = GetNode<TextEdit>("%TextEdit");
		ScriptEditNode = GetNode<CodeEdit>("%ScriptEdit");
		PowerEditNode = GetNode<SpinBox>("%PowerEdit");
		LifeEditNode = GetNode<SpinBox>("%LifeEdit");
		PreviewCardNode = GetNode<Card>("%PreviewCard");
		TypeChecksContainerNode = GetNode<Container>("%TypeChecksContainer");
		SubtypeChecksContainerNode = GetNode<Container>("%SubtypeChecksContainer");

		ConfirmDiscardPopupNode = GetNode<ConfirmationDialog>("%ConfirmDiscardPopup");
		PreviewWindowNode = GetNode<Window>("%PreviewWindow");
		
		#endregion

		foreach (var pair in TYPE_MAP) {
			var type = pair.Key;
            var check = new CheckBox
            {
				Text = type,
                SizeFlagsHorizontal = SizeFlags.Expand,
            };
			check.SetMeta("Value", type);

			TypeChecksContainerNode.AddChild(check);
			_typeCheckBoxes.Add(check);
			check.Toggled += (toggled) => {
				if (toggled) {
					foreach (var c in _typeCheckBoxes) {
						if (c == check) continue;
						c.ButtonPressed = false;
					}
				}

				var validSubtypes = TYPE_MAP[type];
				foreach (var c in _subtypeCheckBoxes) {
					c.Visible = false;
					c.ButtonPressed = false;
					if (!toggled || !validSubtypes.Contains(c.GetMeta("Value").AsString())) continue;

					c.Visible = true;
				}
				PreviewCardNode.Load(Baked);
			};

			foreach (var st in pair.Value) {
				var subcheck = new CheckBox
				{
					Text = st,
					SizeFlagsHorizontal = SizeFlags.Expand,
				};
				subcheck.SetMeta("Value", st);

				SubtypeChecksContainerNode.AddChild(subcheck);
				_subtypeCheckBoxes.Add(subcheck);
				subcheck.Toggled += (toggled) => {
					PreviewCardNode.Load(Baked);
				};
				subcheck.Visible = false;
			}
        }
	}

	#nullable enable
	public void Load(HexCore.Cards.ExpansionCard? card) {
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

	private void SetData(HexCore.Cards.ExpansionCard card) {
		NameEditNode.Text = card.Name;
		CostEditNode.Value = card.Cost;
		DeckUsableCheckNode.ButtonPressed = card.DeckUsable;
		CardType = card.Type;
		TextEditNode.Text = card.Text;
		ScriptEditNode.Text = card.Script;
		PowerEditNode.Value = card.Power;
		LifeEditNode.Value = card.Life;
		ExpansionEditNode.Text = card.Expansion;

		PreviewCardNode.Load(card);
	}
	
	public HexCore.Cards.ExpansionCard Baked {
		get => new()
		{
			Name = NameEditNode.Text,
			Power = (int)PowerEditNode.Value,
			Life = (int)LifeEditNode.Value,
			Cost = (int)CostEditNode.Value,
			Type = CardType,
			Text = TextEditNode.Text,
			Script = ScriptEditNode.Text,
			Expansion = ExpansionEditNode.Text,
		};
	}

	private bool Compare() {
		var card = Baked;
		HexCore.Cards.ExpansionCard oldCard = _edited;

		if (card.Name != oldCard.Name) return true;
		if (card.Cost != oldCard.Cost) return true;
		if (card.Expansion != oldCard.Expansion) return true;
		if (card.Type != oldCard.Type) return true;
		if (card.Text != oldCard.Text) return true;
		if (card.Power != oldCard.Power) return true;
		if (card.Life != oldCard.Life) return true;
		if (card.DeckUsable != oldCard.DeckUsable) return true;
		if (card.Script != oldCard.Script) return true;

		return false;
	}

	public void TryClose() {
		var changesPresent = true;
		if (_edited is not null)
			changesPresent = Compare();
		if (changesPresent) {
			ConfirmDiscardPopupNode.Show();
			return;
		}

		EmitSignal(SignalName.Closed);
	}
	
	#region Signal connections
	
	private void OnSaveButtonPressed()
	{
		// * validation is currently done server-side, the window will not close unless user receives 200
		
		var oldName = _edited is not null ? $"{_edited.Expansion}::{_edited.Name}" : "";

		var result = Baked;

		EmitSignal(SignalName.Saved, new Wrapper<HexCore.Cards.ExpansionCard>(result), oldName);
	}

	private void OnCancelButtonPressed()
	{
		TryClose();
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
			var card = JsonSerializer.Deserialize<HexCore.Cards.ExpansionCard>(data);
			
			SetData(card);
		} catch {}
	}

	private void OnConfirmDiscardPopupConfirmed()
	{
		EmitSignal(SignalName.Closed);
	}

	private void OnOpenPreviewButtonPressed()
	{
		PreviewWindowNode.Show();
	}

	private void OnPreviewWindowCloseRequested()
	{
		PreviewWindowNode.Hide();
	}

	private void OnClosed()
	{
		PreviewWindowNode.Hide();
	}

	private void OnNameEditTextChanged(string new_text)
	{
		PreviewCardNode.Load(Baked);
	}

	private void OnExpansionEditTextChanged(string new_text)
	{
		PreviewCardNode.Load(Baked);
	}

	private void OnCostEditValueChanged(double value)
	{
		PreviewCardNode.Load(Baked);
	}

	private void OnPowerEditValueChanged(double value)
	{
		PreviewCardNode.Load(Baked);
	}

	private void OnLifeEditValueChanged(double value)
	{
		PreviewCardNode.Load(Baked);
	}

	private void OnDeckUsableCheckToggled(bool button_pressed)
	{
		PreviewCardNode.Load(Baked);
	}

	private void OnTextEditTextChanged()
	{
		PreviewCardNode.Load(Baked);
	}

	private void OnScriptEditTextChanged()
	{
		PreviewCardNode.Load(Baked);
	}

	private void OnSaved(Node card, string oldName)
	{
		PreviewWindowNode.Hide();
	}
	
	#endregion
}

