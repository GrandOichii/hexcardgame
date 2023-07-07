using Godot;
using System;

using core.match.states;
using System.Collections.Generic;
using core.tiles;

public partial class TileBase : Node2D, IGamePart
{
	private Polygon2D Bg;
	private Polygon2D Fg;
	private Label CoordsLabel;
	private Label PowerLabel;
	private Label LifeLabel;
	private Label DefenceLabel;	
	private CardBase HoverCard;
	private MarginContainer SizeContainer;

	public TileState? LastState { get; private set; }
	
	private Color BaseColor = new Color(0, 0, 0);
	private Color HoverColor = new Color(0, 0, 1);

	static private readonly Dictionary<string, Color> PlayerColors = new() {
		{"", new Color(1, 1, 1)},		
		{"1", new Color(1, 0, 0)},
		{"2", new Color(0, 1, 0)},
	};

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Bg = GetNode<Polygon2D>("%Bg");
		Fg = GetNode<Polygon2D>("%Fg");
		SizeContainer = GetNode<MarginContainer>("%SizeContainer");		
		PowerLabel = GetNode<Label>("%PowerLabel");
		LifeLabel = GetNode<Label>("%LifeLabel");
		DefenceLabel = GetNode<Label>("%DefenceLabel");
		
		CoordsLabel = GetNode<Label>("%CoordsLabel");
		HoverCard = GetNode<CardBase>("%HoverCard");

	}

	private Vector2 _coords;
	public Vector2 Coords {
		get => _coords;
		set {
			_coords = value;
			
			CoordsLabel.Text = _coords.Y + "." + _coords.X;
			if (_coords.Y < Game.Instance.LastState.Map.Tiles.Count / 2) {
				HoverCard.Position = new Vector2(HoverCard.Position.X, 66);
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
		var powerS = "";
		var lifeS = "";
		var defenceS = "";
		var en = state?.Entity;
		if (en is not null) {
			MCardState card = (MCardState)en;
			lifeS = card.Life.ToString();
			if (card.Power > 0)
				powerS = card.Power.ToString();
			if (card.HasDefence)
				defenceS = "+" + card.Defence.ToString();
			HoverCard.Load(card);
		}
		PowerLabel.Text = powerS;
		LifeLabel.Text = lifeS;
		DefenceLabel.Text = defenceS;	
	}

	private string _playerID;
	public string PlayerID {
		get => _playerID;
		set {
			_playerID = value;
			var color = PlayerColors[value];
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
				// OnCollisionMouseExited();

// 				if (action.Count == 0) {
// 					game.AddToAction("move");
// 					game.AddToAction("" + _coords.Y + "." + _coords.X);
// 					return;
// 				}
// 				if (action.Count == 2)  {
// 					if (action[0] == "play") {
// 						game.AddToAction("" + _coords.Y + "." + _coords.X);
// 						game.SendAction();
// 						return;
// 					}
// 					if (action[0] == "move") {
// 						GD.Print("MOVE");
// 						var all_dir_arr = core.tiles.Map.DIR_ARR;
// 						var ii = (int)_coords.Y % 2;
// 						var dir_arr = all_dir_arr[ii];
// 						for (int i = 0; i < dir_arr.Length; i++) {
// //							GD.Print(_coords.Y, " ", _coords.X);
// 							var newC = new Vector2((int)_coords.X + (int)dir_arr[i][1], (int)_coords.Y + dir_arr[i][0]);
// //							GD.Print(newC, "  ", dir_arr[i][0], "  ", dir_arr[i][1]);
// 							var tS = "" + newC.Y + "." + newC.X;
// 							GD.Print(i, " ", tS, " ", action[1]);
// 							if (action[1] == tS) {
// 								game.AddToAction(((i + 3)%6).ToString());
// 								game.SendAction();
// 								return;
// 							}
// 						}
// 						game.SendAction();
// 						return;
// 					}
// 				}
			}
		}
//		if (@event is InputEventMouseMotion) {
//			var e = @event as InputEventMouseMotion;
//			if (e.ShiftPressed && LastState?.Entity is not null) {
//				HoverCardBase hc = Game.Instance.HoverCard;
//				hc.Visible = true;
//				MCardState en = (MCardState)(LastState?.Entity);
//				hc.Load(en);
//				GD.Print(hc.ZIndex, " ", ZIndex);
//			}
//		}
	}
	private void OnCollisionMouseEntered()
	{
		if (LastState is not null && LastState?.Entity is not null) {
			HoverCard.Visible = true;
		}
		
		if (!Game.Instance.Accepts(this)) return;

		Bg.Color = HoverColor;

	}
	private void OnCollisionMouseExited()
	{
		Bg.Color = BaseColor;	
		if (HoverCard.Visible)
			HoverCard.Visible = false;
	}

	// public string ToActionPart(Command command)
	// {
	// 	// TODO? more complex behavior (if need entity and not card)
	// 	return "" + _coords.Y + "." + _coords.X;
	// }

}




