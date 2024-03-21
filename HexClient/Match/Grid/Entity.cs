using Godot;
using HexCore.GameMatch.States;
using System;
using System.Collections.Generic;

namespace HexClient.Match.Grid;

public partial class Entity : Node2D, IEntity
{
	#region Nodes
	
	public Label PowerLabelNode { get; private set; }
	public Label LifeLabelNode { get; private set; }
	public Label DefenceLabelNode { get; private set; }
	public Control MoveIndicatorNode { get; private set; }
	public Polygon2D BgNode { get; private set; }
	public Label IDLabelNode { get; private set; }

	#endregion

	private Dictionary<string, Color> _playerColors = new();

	public MatchCardState State { get; set; }
	// public MatchConnection Client { get; set; }

	private string _ownerId;
	public string OwnerId {
		get => _ownerId;
		set {
			_ownerId = value;
			if (string.IsNullOrEmpty(value) || !_playerColors.ContainsKey(value)) return;

			var color = _playerColors[value];
			BgNode.Color = color.Darkened(.2f);
		}
	}
	
	public override void _Ready()
	{
		#region Node fetching
		
		PowerLabelNode = GetNode<Label>("%PowerLabel");
		LifeLabelNode = GetNode<Label>("%LifeLabel");
		DefenceLabelNode = GetNode<Label>("%DefenceLabel");
		MoveIndicatorNode = GetNode<Control>("%MoveIndicator");
		BgNode = GetNode<Polygon2D>("%Bg");
		IDLabelNode = GetNode<Label>("%IDLabel");
		
		#endregion
	}

	public void Load(MatchCardState state) {
		State = state;

		OwnerId = state.OwnerID;

		IDLabelNode.Text = state.ID;

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

	public void TweenPosition(Vector2 newPos, double duration)
	{
		CreateTween().TweenProperty(this, "position", newPos, duration);
	}

	public void SetPosition(Vector2 pos) {
		Position = pos;
	}

	public void SetPlayerColors(Dictionary<string, Color> colors){
		_playerColors = colors;
		OwnerId = _ownerId;
	}
}
