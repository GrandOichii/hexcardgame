using Godot;
using System;

using core.match.states;
using System.Collections.Generic;

public partial class TileBase : Node2D
{
	private Polygon2D Bg;
	private Polygon2D Fg;
	private Label CoordsLabel;
	private Label PowerLabel;
	private Label LifeLabel;
	private MarginContainer SizeContainer;

	private TileState? _lastState;
	
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
		CoordsLabel = GetNode<Label>("%CoordsLabel");

	}

	private Vector2 _coords;
	public Vector2 Coords {
		get => _coords;
		set {
			_coords = value;
			CoordsLabel.Text = _coords.Y + "." + _coords.X;
		}
	} 

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
//		Bg.Color = new Color((float)Rnd.NextDouble(), (float)Rnd.NextDouble(), (float)Rnd.NextDouble());
	}

	public void Load(TileState? state) {
		_lastState = state;
		if (state is null) {
			// PlayerID = "";
			Visible = false;

			return;
		}
		PlayerID = state?.OwnerID;
		var powerS = "";
		var lifeS = "";
		var en = state?.Entity;
		if (en is not null) {
			var card = en;
			lifeS = card?.Life.ToString();
			if (card?.Power > 0)
				powerS = card?.Power.ToString();
		}
		PowerLabel.Text = powerS;
		LifeLabel.Text = lifeS;
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
		if (@event is InputEventMouseButton) {
			var e = @event as InputEventMouseButton;
			if (e.IsPressed() && e.ButtonIndex == MouseButton.Left) {
				GD.Print(_coords);
			}
		}
		if (@event is InputEventMouseMotion) {
			var e = @event as InputEventMouseMotion;
			if (e.ShiftPressed) {
				GD.Print("MO");
			}
		}
	}
	private void OnCollisionMouseEntered()
	{
		Bg.Color = HoverColor;
		// Replace with function body.
	}
	private void OnCollisionMouseExited()
	{
		Bg.Color = BaseColor;	
	}
	
}




