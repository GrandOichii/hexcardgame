using Godot;
using System;

namespace HexClient.Manager;

public partial class ActionDisplay : Control, IActionDisplay
{
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

		// CustomMinimumSize = new(140, 40);
		// GD.Print(Size);
	}

	public void Load(RecordedAction action) {
		ActionLabelNode.Text = action.Action;
		// TODO
	}
}
