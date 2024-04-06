using Godot;
using System;
using System.Collections.Generic;

namespace HexClient.Manager;

public partial class ActionDisplay : Control, IActionDisplay
{
	private Dictionary<string, Color> _colorMap = new();

	#region Nodes

	public PanelContainer BgNode { get; private set; }
	public Label ActionLabelNode { get; private set; }

	#endregion

	public override void _Ready()
	{
		#region Node fetching

		BgNode = GetNode<PanelContainer>("%Bg");
		ActionLabelNode = GetNode<Label>("%ActionLabel");

		#endregion

		BgNode.Set("theme_override_styles/panel", BgStyle.Duplicate());
	}

	public Color BgColor {
		get => BgStyle.BgColor;
		set {
			BgStyle.BgColor = value;
		}
	}

	private StyleBoxFlat BgStyle => BgNode.Get("theme_override_styles/panel").As<StyleBoxFlat>();

	private RecordedAction _action;

	public void Load(RecordedAction action) {
		_action = action;
		ActionLabelNode.Text = action.Action;
		
		if (_colorMap.ContainsKey(action.PlayerId))
			BgColor = _colorMap[action.PlayerId];
	}

	public void OnPlayerColorsUpdated(Wrapper<Dictionary<string, Color>> mapW)
	{
		_colorMap = mapW.Value;
		Load(_action);
	}
}
