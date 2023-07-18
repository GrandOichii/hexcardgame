using core.match.states;
using Godot;
using System;

public partial class EntityBase : Node2D, IGamePart
{
	public MCardState LastState { get; private set; }


	private Polygon2D[] timerNodes;
	// private Node2D timerNodes;
	private Polygon2D BgNode;
	private Panel CanMoveNode;
	private Label PowerLabel;
	private Label LifeLabel;
	private Label DefenceLabel;

	public override void _Ready()
	{
		// tween.TweenProperty(this, "position", new Vector2(0, 200), .5);
		// tween.TweenProperty(this, "position", new Vector2(200, 0), .5);

		BgNode = GetNode<Polygon2D>("%Bg");
		CanMoveNode = GetNode<Panel>("%CanMoveRect");
		PowerLabel = GetNode<Label>("%PowerLabel");
		LifeLabel = GetNode<Label>("%LifeLabel");
		DefenceLabel = GetNode<Label>("%DefenceLabel");
		
		var tNode = GetNode<Node2D>("%TimerPolys");
		var count = tNode.GetChildCount();
		timerNodes = new Polygon2D[count];
		for (int i = 0; i < count; i++) {
			timerNodes[i] = tNode.GetChild(i) as Polygon2D;
		}

		SetTimer(0);
	}

	public void SetTimer(int value) {
		for (int i = 0; i < timerNodes.Length; i++) {
			var n = timerNodes[i];
			n.Visible = false;
			if (i < value) n.Visible = true;
		}
	}

	public void Load(MCardState state) {
		LastState = state;

		var color = Game.Instance.PlayerColors[state.OwnerID];
		BgNode.Color = color.Darkened(.2f);

		var powerS = "";
		var defenceS = "";
		var lifeS = state.Life.ToString();
		if (state.Power > 0)
			powerS = state.Power.ToString();
		if (state.HasDefence)
			defenceS = "+" + state.Defence.ToString();

		PowerLabel.Text = powerS;
		LifeLabel.Text = lifeS;
		DefenceLabel.Text = defenceS;	

		// TODO sometimes is still present for some reason
		CanMoveNode.Visible = state.Movement > 0;
	}
}
