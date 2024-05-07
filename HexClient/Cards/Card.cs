using Godot;
using HexCore.Cards;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HexClient.Cards;

/// <summary>
/// Control node, displays a card
/// </summary>
public partial class Card : Control
{
	/// <summary>
	/// Keyword to bbcode color mapping
	/// </summary>
	private readonly Dictionary<string, string> KEYWORD_COLOR_MAPPING = new() {
		{ "Fast", "plum" },
		{ "Vile", "darkred" },
		{ "Virtuous", "lightskyblue" },
	};

	#region Exports
	
	/// <summary>
	/// Export value, represents the color of the border when card is focused
	/// </summary>
	[Export]
	public Color FocusColor { get; set; }
	
	#endregion

	#region Nodes
	
	/// <summary>
	/// Background node
	/// </summary>
	public PanelContainer BgNode { get; private set; }

	/// <summary>
	/// Foreground node
	/// </summary>
	public PanelContainer FgNode { get; private set; }

	/// <summary>
	/// Name label node
	/// </summary>
	public Label NameLabelNode { get; private set; }

	/// <summary>
	/// Cost label node
	/// </summary>
	public Label CostLabelNode { get; private set; }

	/// <summary>
	/// Card image node
	/// </summary>
	public TextureRect ImageNode { get; private set; }

	/// <summary>
	/// Card type node
	/// </summary>
	public Label TypeLabelNode { get; private set; }

	/// <summary>
	/// Bottom information node of the card
	/// </summary>
	public Control BottomNode { get; private set; }

	/// <summary>
	/// Card power label node
	/// </summary>
	public Label PowerLabelNode { get; private set; }

	/// <summary>
	/// Card life label node
	/// </summary>
	public Label LifeLabelNode { get; private set; }

	/// <summary>
	/// Card defence label node
	/// </summary>
	public Label DefenceLabelNode { get; private set; }

	/// <summary>
	/// Card text label node
	/// </summary>
	public RichTextLabel TextLabelNode { get; private set; }

	/// <summary>
	/// Power-life separator node
	/// </summary>
	public Control PLSeparatorNode { get; private set; }
	
	#endregion

	/// <summary>
	/// Default background color
	/// </summary>
	private Color _defaultBgColor;

	/// <summary>
	/// Match card state (for match cards)
	/// </summary>
	public HexStates.MatchCardState? State { get; private set; }

	/// <summary>
	/// Card state (for general cards)
	/// </summary>
	public HexCore.Cards.Card CardState { get; private set; }

	/// <summary>
	/// Flag for showing card match ID
	/// </summary>
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

	/// <summary>
	/// Function to be executed before loading a new state
	/// </summary>
	private void PreLoad() {
		BottomNode.Show();
		PLSeparatorNode.Show();
		PowerLabelNode.Show();
	}

	/// <summary>
	/// Sets the type of the card
	/// </summary>
	/// <param name="type">Card type</param>
	private void SetType(string type) {
		TypeLabelNode.Text = type;

		if (type.StartsWith("Spell"))
			BottomNode.Hide();
		if (type.StartsWith("Structure")) {
			PLSeparatorNode.Hide();
			PowerLabelNode.Hide();
		}
	}

	/// <summary>
	/// Loads the match card state
	/// </summary>
	/// <param name="cardState">Match card state</param>
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

	/// <summary>
	/// Loads the general card info
	/// </summary>
	/// <param name="card">Card info</param>
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

	/// <summary>
	/// Sets the card text
	/// </summary>
	/// <param name="name">Card name</param>
	/// <param name="text">Card text</param>
	private void SetText(string name, string text) {
		foreach (var pair in KEYWORD_COLOR_MAPPING) {
			var keyword = pair.Key;
			var color = pair.Value;
			text = Regex.Replace(text, @"\{" + keyword + @"\}", $"[color={color}]{keyword}[/color]");
		}
		text = Regex.Replace(text, @"\[CARDNAME\]", $"[color=orange]{name}[/color]");
		TextLabelNode.Text = text;

	}

	/// <summary>
	/// Background color
	/// </summary>
	public Color BgColor {
		get => BgStyle.BgColor;
		set {
			BgStyle.BgColor = value;
		}
	}

	/// <summary>
	/// Background style
	/// </summary>
	private StyleBoxFlat BgStyle => BgNode.Get("theme_override_styles/panel").As<StyleBoxFlat>();

	/// <summary>
	/// Unfocuses the card
	/// </summary>
	public void Unfocus() {
		CreateTween().TweenProperty(this, "BgColor", _defaultBgColor, .1f);
	}

	/// <summary>
	/// Focuses on the card
	/// </summary>
	public void Focus() {
		CreateTween().TweenProperty(this, "BgColor", FocusColor, .1f);
	}
	
	/// <summary>
	/// Setter for the ShowMID flag
	/// </summary>
	/// <param name="value">new value</param>
	public void SetShowMID(bool value) {
		ShowMID = value;

		if (State is {} state)
			Load(state);
			return;		
	}
	
	#region Signal connections


	#endregion
}

