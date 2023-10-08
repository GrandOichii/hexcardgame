using core.match.states;
using Godot;
using System;

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
		
		// TODO don't know if this is correct, don't see any other way
		BgNode.Set("theme_override_styles/panel", _bgStyle.Duplicate());

		_defaultBgColor = BgColor;
	}

	public void Load(MCardState cardState) {
		NameLabelNode.Text = cardState.Name;
		CostLabelNode.Text = " " + cardState.Cost.ToString() + " ";
		TypeLabelNode.Text = cardState.Type;
		PowerLabelNode.Text = cardState.Power.ToString();
		LifeLabelNode.Text = cardState.Life.ToString();
		DefenceLabelNode.Text = cardState.Defence.ToString();
		TextLabelNode.Text = cardState.Text;
	}
	
	public Color BgColor {
		get => _bgStyle.BgColor;
		set {
			_bgStyle.BgColor = value;
		}
	}

	private StyleBoxFlat _bgStyle => BgNode.Get("theme_override_styles/panel").As<StyleBoxFlat>();

	#region Signal connections

	private void _on_mouse_entered()
	{
		CreateTween().TweenProperty(this, "BgColor", new Color(1, 0, 0), .1f);
	}

	private void _on_mouse_exited()
	{
		CreateTween().TweenProperty(this, "BgColor", _defaultBgColor, .1f);	
	}

	#endregion
}


