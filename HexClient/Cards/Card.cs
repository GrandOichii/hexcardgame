using Godot;
using HexCore.Cards;
using System;

namespace HexClient.Cards;

public partial class Card : Control
{

	#region Nodes
	
	public PanelContainer BgNode { get; private set; }
	public PanelContainer FgNode { get; private set; }
	public Label NameLabelNode { get; private set; }
	public Label CostLabelNode { get; private set; }
	public TextureRect ImageNode { get; private set; }
	public Label TypeLabelNode { get; private set; }
	public Control BottomNodeNode { get; private set; }
	public Label PowerLabelNode { get; private set; }
	public Label LifeLabelNode { get; private set; }
	public Label DefenceLabelNode { get; private set; }
	public RichTextLabel TextLabelNode { get; private set; }
	
	#endregion

	private Color _defaultBgColor;
	public HexStates.MatchCardState State { get; private set; }
	public HexCore.Cards.Card CardState { get; private set; }
	// public MatchConnection Client { get; set; }

	public override void _Ready()
	{
		#region Node fetching

		BgNode = GetNode<PanelContainer>("%Bg");
		FgNode = GetNode<PanelContainer>("%Fg");
		NameLabelNode = GetNode<Label>("%NameLabel");
		CostLabelNode = GetNode<Label>("%CostLabel");
		ImageNode = GetNode<TextureRect>("%Image");
		TypeLabelNode = GetNode<Label>("%TypeLabel");
		BottomNodeNode = GetNode<Control>("%Bottom");
		PowerLabelNode = GetNode<Label>("%PowerLabel");
		LifeLabelNode = GetNode<Label>("%LifeLabel");
		DefenceLabelNode = GetNode<Label>("%DefenceLabel");
		TextLabelNode = GetNode<RichTextLabel>("%TextLabel");

		#endregion
		
		// don't know if this is correct, don't see any other way
		BgNode.Set("theme_override_styles/panel", BgStyle.Duplicate());

		_defaultBgColor = BgColor;
	}

	public void Load(HexStates.MatchCardState cardState) {
		State = cardState;
		NameLabelNode.Text = cardState.Name + " [" + cardState.MID + "]";
		CostLabelNode.Text = " " + cardState.Cost.ToString() + " ";
		TypeLabelNode.Text = cardState.Type;
		PowerLabelNode.Text = cardState.Power.ToString();
		LifeLabelNode.Text = cardState.Life.ToString();
		DefenceLabelNode.Text = cardState.Defence.ToString();
		TextLabelNode.Text = cardState.Text;
	}

	public void Load(HexCore.Cards.Card card) {
		CardState = card;
		NameLabelNode.Text = card.Name;
		CostLabelNode.Text = " " + card.Cost.ToString() + " ";
		TypeLabelNode.Text = card.Type;
		PowerLabelNode.Text = card.Power.ToString();
		LifeLabelNode.Text = card.Life.ToString();
		DefenceLabelNode.Text = "";
		TextLabelNode.Text = card.Text;
	}

	public Color BgColor {
		get => BgStyle.BgColor;
		set {
			BgStyle.BgColor = value;
		}
	}

	private StyleBoxFlat BgStyle => BgNode.Get("theme_override_styles/panel").As<StyleBoxFlat>();

	// public void Unfocus() {
	// 	CreateTween().TweenProperty(this, "BgColor", _defaultBgColor, .1f);	
	// }

	// public void Focus() {
	// 	CreateTween().TweenProperty(this, "BgColor", FocusColor, .1f);
	// }
	
	#region Signal connections
	
	private void OnGuiInput(InputEvent @event)
	{
		// Replace with function body.
	}
	
	#endregion
}

