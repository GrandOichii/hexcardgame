using Godot;
using System;
using core.match.states;
using System.Collections.Generic;
using core.tiles;

public partial class TileBase : Node2D, IGamePart
{
#nullable enable
	public EntityBase? Entity { get; set; } = null;
#nullable disable

	private Polygon2D Bg;
	private Polygon2D Fg;
	private Label CoordsLabel;
	// private Label PowerLabel;
	// private Label LifeLabel;
	// private Label DefenceLabel;	
	private CardBase HoverCard;
	private MarginContainer SizeContainer;
	
	public Map Map { get; set; }

	public TileState? LastState { get; private set; }
	
	private Color BaseColor = new Color(0, 0, 0);
	private Color HoverColor = new Color(0, 0, 1);


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Bg = GetNode<Polygon2D>("%Bg");
		Fg = GetNode<Polygon2D>("%Fg");
		SizeContainer = GetNode<MarginContainer>("%SizeContainer");		
		// PowerLabel = GetNode<Label>("%PowerLabel");
		// LifeLabel = GetNode<Label>("%LifeLabel");
		// DefenceLabel = GetNode<Label>("%DefenceLabel");
		
		CoordsLabel = GetNode<Label>("%CoordsLabel");
		HoverCard = GetNode<CardBase>("%HoverCard");

	}

	public string CoordsStr => _coords.Y + "." + _coords.X;

	private Vector2 _coords;
	public Vector2 Coords {
		get => _coords;
		set {
			_coords = value;
			
			CoordsLabel.Text = CoordsStr;
			if (_coords.Y < Game.Instance.LastState.Map.Tiles.Count / 2) {
				HoverCard.Position = new Vector2(HoverCard.Position.X, 138);
			}
		}
	} 

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
//		Bg.Color = new Color((float)Rnd.NextDouble(), (float)Rnd.NextDouble(), (float)Rnd.NextDouble());
	}

	public void Load(TileState? state) {
		LastState = state;
		if (state is null) {
			// PlayerID = "";
			Visible = false;

			return;
		}
		PlayerID = state?.OwnerID;
		// var powerS = "";
		// var lifeS = "";
		// var defenceS = "";
		var en = state?.Entity;
		if (en is not null) {
			MCardState card = (MCardState)en;
			// lifeS = card.Life.ToString();
			// if (card.Power > 0)
			// 	powerS = card.Power.ToString();
			// if (card.HasDefence)
			// 	defenceS = "+" + card.Defence.ToString();
			HoverCard.Load(card);
		}
		// PowerLabel.Text = powerS;
		// LifeLabel.Text = lifeS;
		// DefenceLabel.Text = defenceS;	
	}

	private string _playerID;
	public string PlayerID {
		get => _playerID;
		set {
			_playerID = value;
			var color = Game.Instance.PlayerColors[value];
			Bg.Color = new Color(0, 0, 0);
			Fg.Color = color;
		}
	}
//	private void OnArea2dInputEvent(Node viewport, InputEvent @event, long shape_idx)
//	{
//
//		if (@event is InputEventMouseButton) {
//			var e = @event as InputEventMouseButton;
//			if (e.IsPressed() && e.ButtonIndex == MouseButton.Left) {
//				GD.Print(_coords);
//			}
//		}
//		// Replace with function body.
//	}
	static private Random Rnd = new();
	
	public Vector2 Size => SizeContainer.Size * Scale;


	private void OnCollisionInputEvent(Node viewport, InputEvent @event, long shape_idx)
	{
		// OnCollisionMouseEntered();
		if (@event is InputEventMouseButton) {
			var e = @event as InputEventMouseButton;
// 			if (e.ShiftPressed && LastState?.Entity is not null) {				
// //				HoverCardBase hc = Game.Instance.HoverCard;
// //				hc.Visible = true;
// //				MCardState en = (MCardState)(LastState?.Entity);
// //				hc.Load(en);
// //				GD.Print(hc.ZIndex, " ", ZIndex);
// 				return;
// //			}
			if (e.IsPressed() && e.ButtonIndex == MouseButton.Left) {
				var game = Game.Instance;
				if (!game.Accepts(this)) return;

				game.Process(this);
				Map.MovementArrow.Visible = false;
			}
		}
	}
	private readonly float ANGLE = (float)Math.PI / 3;
	private readonly float ANGLE_OFFSET = (float)Math.PI / 6 + (float)Math.PI / 3;
	
	private void OnCollisionMouseEntered()
	{
		if (LastState is not null && LastState?.Entity is not null) {
			HoverCard.Visible = true;
		}
		
		if (!Game.Instance.Accepts(this)) return;

		Bg.Color = HoverColor;

		var command = Game.Instance.CurrentCommand;
		if (command is null || command.Name != "move") {
			return;
		}
		var arrow = Map.MovementArrow;
		arrow.Visible = true;
		var tile = Game.Instance.CurrentCommand.Results[0] as TileBase;
		arrow.Position = tile.Position + new Vector2(Size.X / 2, Size.Y / 2);
		var dir = SelectDirection.GetDirection(tile, this);
		arrow.Rotation = ANGLE * dir - ANGLE_OFFSET;

	}
	private void OnCollisionMouseExited()
	{
		Bg.Color = BaseColor;
		if (HoverCard.Visible)
			HoverCard.Visible = false;
		if (Map.MovementArrow.Visible) Map.MovementArrow.Visible = false;
	}

	// public string ToActionPart(Command command)
	// {
	// 	// TODO? more complex behavior (if need entity and not card)
	// 	return "" + _coords.Y + "." + _coords.X;
	// }

}




