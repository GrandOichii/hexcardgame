using Godot;
using HexCore.GameMatch.States;
using System;

namespace HexClient.Match.Grid;

public partial class Entity : Node2D
{
	#region Nodes
	
	public Label PowerLabelNode { get; private set; }
	public Label LifeLabelNode { get; private set; }
	public Label DefenceLabelNode { get; private set; }
	public Control MoveIndicatorNode { get; private set; }
	public Polygon2D BgNode { get; private set; }

	#endregion

	public MatchCardState State { get; set; }
	// public MatchConnection Client { get; set; }
	
	public override void _Ready()
	{
		#region Node fetching
		
		PowerLabelNode = GetNode<Label>("%PowerLabel");
		LifeLabelNode = GetNode<Label>("%LifeLabel");
		DefenceLabelNode = GetNode<Label>("%DefenceLabel");
		MoveIndicatorNode = GetNode<Control>("%MoveIndicator");
		BgNode = GetNode<Polygon2D>("%Bg");
		
		#endregion
	}

	public void Load(MatchCardState state) {
		State = state;

		// var color = Client.PlayerColors[state.OwnerID];
		// BgNode.Color = color.Darkened(.2f);

		PowerLabelNode.Text = state.Power.ToString();
		LifeLabelNode.Text = state.Life.ToString();

		var powerS = "";
		var defenceS = "";
		var lifeS = state.Life.ToString();
		if (state.Power > 0)
			powerS = state.Power.ToString();
		if (state.HasDefence)
			defenceS = "+" + state.Defence.ToString();

		PowerLabelNode.Text = powerS;
//		PowerLabelNode.Visible = powerS.Length != 0;

		LifeLabelNode.Text = lifeS;
		DefenceLabelNode.Text = defenceS;

		MoveIndicatorNode.Visible = state.Movement > 0;
	}
}
