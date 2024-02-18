using Godot;
using System;
using System.Reflection;

public partial class Tile : Node2D, IGamePart
{
	#region Exports
	
	[Export]
	public Color HighlightColor { get; set; }
	
	#endregion
	
	#region Nodes
	
	public Polygon2D BgNode { get; private set; }
	public Polygon2D FgNode { get; private set; }
	public Control BBoxNode { get; private set; }
	
	public CollisionPolygon2D CollisionPolyNode { get; private set; }
	
	#endregion
	public TileState? State { get; private set; }
	
	private Color _defaultBgColor;
	public Vector2 Size => BBoxNode.Size;
	public Vector2 Coords { get; set; }
	public Entity Entity { get; set; }
	public MatchConnection Client { get; set; }
	
	public override void _Ready()
	{
		#region Node fetching
		
		BgNode = GetNode<Polygon2D>("%Bg");
		FgNode = GetNode<Polygon2D>("%Fg");
		BBoxNode = GetNode<Control>("%BBox");
		CollisionPolyNode = GetNode<CollisionPolygon2D>("%CollisionPoly");
		
		#endregion
		
		// CollisionPolyNode.SetProcess(true);
		
//		GD.Print(GetNode<Area2D>("%Collision").Monitoring);
		
		_defaultBgColor = BgNode.Color;
	}

	public override void _Input(InputEvent e)
	{
		if (e.IsActionPressed("cancel-command"))
			Recheck();
	}

	public string CoordsStr => Coords.Y + "." + Coords.X;

	private string _playerID;
	public string PlayerID {
		get => _playerID;
		set {
			_playerID = value;
			var color = Client.PlayerColors[value];
			// BgNode.Color = new Color(0, 0, 0);
			FgNode.Color = color;
		}
	}
	
	public void Recheck() {
		Unfocus();
		// _on_collision_mouse_entered();
	}

	public void Load(TileState? state) {
		State = state;
		if (state is null) {
			Visible = false;
			return;
		}

		Visible = true;
		PlayerID = state?.OwnerID;
	}
	
	private void _pressed() {
		if (!Client.Accepts(this)) return;

		Client.Process(this);
		// Map.MovementArrow.Visible = false;
		Recheck();
	}
	
	private void Unfocus() {
		BgNode.Color = _defaultBgColor;
	}
	
	#region Signal connections

	private void _on_collision_mouse_entered()
	{
		if (State is not null && State?.Entity is not null) {
			MatchCardState card = (MatchCardState)(State?.Entity);
			Client.HoverCard.Load(card);
			// HoverCard.Visible = true;
		}
		if (!Client.Accepts(this)) return;

		BgNode.Color = HighlightColor;

		var command = Client.CurrentCommand;
		if (command is null || command.Name != "move") {
			return;
		}

		// TODO restore
		// var arrow = Map.MovementArrow;
		// arrow.Visible = true;
		// var tile = Game.Instance.CurrentCommand.Results[0] as TileBase;
		// arrow.Position = tile.Position + new Vector2(Size.X / 2, Size.Y / 2);
		// var dir = SelectDirection.GetDirection(tile, this);
		// arrow.Rotation = ANGLE * dir - ANGLE_OFFSET;
	}

	private void _on_collision_mouse_exited()
	{
		Unfocus();
		// SetColor(BaseColor);
		// if (HoverCard.Visible)
			// HoverCard.Visible = false;
		// if (Map.MovementArrow.Visible) Map.MovementArrow.Visible = false;
	}

	private void _on_collision_input_event(Node viewport, InputEvent e, long shape_idx)
	{
		if (e.IsActionPressed("add-to-action"))
			_pressed();
	}
	
	#endregion
}


