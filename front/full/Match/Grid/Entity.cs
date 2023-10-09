using core.match.states;
using Godot;
using System;

public partial class Entity : Node2D
{
	#region Nodes
	
	public Label PowerLabelNode { get; private set; }
	public Label LifeLabelNode { get; private set; }
	public Control MoveIndicatorNode { get; private set; }
	
	#endregion

	public MCardState State { get; set; }
	
	public override void _Ready()
	{
		#region Node fetching
		
		PowerLabelNode = GetNode<Label>("%PowerLabel");
		LifeLabelNode = GetNode<Label>("%LifeLabel");
		MoveIndicatorNode = GetNode<Control>("%MoveIndicator");
		
		#endregion
	}

	public void Load(MCardState state) {
		State = state;

		PowerLabelNode.Text = state.Power.ToString();
		LifeLabelNode.Text = state.Life.ToString();
		// LifeLabelNode.Text = state.Life.ToString();
	}
}
