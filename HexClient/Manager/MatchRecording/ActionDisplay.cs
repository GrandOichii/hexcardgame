using Godot;
using System;
using System.Collections.Generic;

namespace HexClient.Manager;

public partial class ActionDisplay : Control, IActionDisplay
{
	private static readonly Dictionary<string, Color> _colorMap = new () {
	};

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

	public void Load(RecordedAction action) {
		// TODO change
		ActionLabelNode.Text = action.Action;

		if (!_colorMap.ContainsKey(action.PlayerName)) {
			GD.Print(action.PlayerName);
			_colorMap[action.PlayerName] = new Color(
				GD.Randf(),
				GD.Randf(),
				GD.Randf()
			);
		}
		
		BgColor = _colorMap[action.PlayerName];
	}
}
