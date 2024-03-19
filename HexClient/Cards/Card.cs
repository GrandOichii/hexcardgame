using Godot;
using HexCore.Cards;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HexClient.Cards;

public partial class Card : Control
{
	private readonly Dictionary<string, string> KEYWORD_COLOR_MAPPING = new() {
		{ "Fast", "plum" },
		{ "Vile", "darkred" },
		{ "Virtuous", "lightskyblue" },
	};
	#region Exports
	
	[Export]
	public Color FocusColor { get; set; }
	
	#endregion

	#region Nodes
	
	public PanelContainer BgNode { get; private set; }
	public PanelContainer FgNode { get; private set; }
	public Label NameLabelNode { get; private set; }
	public Label CostLabelNode { get; private set; }
	public TextureRect ImageNode { get; private set; }
	public Label TypeLabelNode { get; private set; }
	public Control BottomNode { get; private set; }
	public Label PowerLabelNode { get; private set; }
	public Label LifeLabelNode { get; private set; }
	public Label DefenceLabelNode { get; private set; }
	public RichTextLabel TextLabelNode { get; private set; }
	public Control PLSeparatorNode { get; private set; }
	
	#endregion

	private Color _defaultBgColor;
	public HexStates.MatchCardState? State { get; private set; }
	public HexCore.Cards.Card CardState { get; private set; }

	public bool ShowMID { get; private set; } = false;

	public override void _Ready()
	{
		#region Node fetching

		BgNode = GetNode<PanelContainer>("%Bg");
		FgNode = GetNode<PanelContainer>("%Fg");
		NameLabelNode = GetNode<Label>("%NameLabel");
		CostLabelNode = GetNode<Label>("%CostLabel");
		ImageNode = GetNode<TextureRect>("%Image");
		TypeLabelNode = GetNode<Label>("%TypeLabel");
		BottomNode = GetNode<Control>("%Bottom");
		PowerLabelNode = GetNode<Label>("%PowerLabel");
		LifeLabelNode = GetNode<Label>("%LifeLabel");
		DefenceLabelNode = GetNode<Label>("%DefenceLabel");
		TextLabelNode = GetNode<RichTextLabel>("%TextLabel");
		PLSeparatorNode = GetNode<Control>("%PLSeparator");

		#endregion
		
		// don't know if this is correct, don't see any other way
		BgNode.Set("theme_override_styles/panel", BgStyle.Duplicate());

		_defaultBgColor = BgColor;
	}

	private void PreLoad() {
		BottomNode.Show();
		PLSeparatorNode.Show();
		PowerLabelNode.Show();
	}

	private void SetType(string type) {
		TypeLabelNode.Text = type;

		if (type.StartsWith("Spell"))
			BottomNode.Hide();
		if (type.StartsWith("Structure")) {
			PLSeparatorNode.Hide();
			PowerLabelNode.Hide();
		}
	}

	public void Load(HexStates.MatchCardState cardState) {
		State = cardState;
		CardState = null;

		PreLoad();

		NameLabelNode.Text = cardState.Name;
		if (ShowMID)
			NameLabelNode.Text += " [" + cardState.MID + "]";
		CostLabelNode.Text = " " + cardState.Cost.ToString() + " ";
		PowerLabelNode.Text = cardState.Power.ToString();
		LifeLabelNode.Text = cardState.Life.ToString();
		DefenceLabelNode.Text = cardState.Defence.ToString();
		SetText(cardState.Name, cardState.Text);
		SetType(cardState.Type);
	}

	public void Load(HexCore.Cards.Card card) {
		CardState = card;
		State = null;

		PreLoad();

		NameLabelNode.Text = card.Name;
		CostLabelNode.Text = " " + card.Cost.ToString() + " ";
		PowerLabelNode.Text = card.Power.ToString();
		LifeLabelNode.Text = card.Life.ToString();
		DefenceLabelNode.Text = "";
		SetText(card.Name, card.Text);
		SetType(card.Type);
	}

	private void SetText(string name, string text) {
		foreach (var pair in KEYWORD_COLOR_MAPPING) {
			var keyword = pair.Key;
			var color = pair.Value;
			text = Regex.Replace(text, @"\{" + keyword + @"\}", $"[color={color}]{keyword}[/color]");
		}
		text = Regex.Replace(text, @"\[CARDNAME\]", $"[color=orange]{name}[/color]");
		TextLabelNode.Text = text;

	}

	public Color BgColor {
		get => BgStyle.BgColor;
		set {
			BgStyle.BgColor = value;
		}
	}

	private StyleBoxFlat BgStyle => BgNode.Get("theme_override_styles/panel").As<StyleBoxFlat>();

	public void Unfocus() {
		CreateTween().TweenProperty(this, "BgColor", _defaultBgColor, .1f);
	}

	public void Focus() {
		CreateTween().TweenProperty(this, "BgColor", FocusColor, .1f);
	}
	
	public void SetShowMID(bool value) {
		ShowMID = value;

		if (State is {} state)
			Load(state);
			return;		
	}
	
	#region Signal connections


	#endregion
}

