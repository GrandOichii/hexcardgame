using Godot;
using HexCore.GameMatch.States;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HexClient.Match.Grid;

public interface IEntity {
	public void Load(MatchCardState entity);
	public void TweenPosition(Vector2 newPos, double duration);
	public void SetPosition(Vector2 pos);
	public void SetPlayerColors(Dictionary<string, Color> colors);
	public void SetShowId(bool v);
}

public partial class Tile : Node2D, ITile
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
	public Label CoordsLabelNode { get; private set; }
	
	#endregion
	public TileState? State { get; private set; }
	
	private Color _defaultBgColor;
	public Vector2 Size => BBoxNode.Size;
	public Vector2 Coords { get; set; }
	public IEntity Entity { get; set; }
	// public MatchConnection Client { get; set; }

	private Dictionary<string, Color> _playerColors = new();

	private CommandProcessor? _processor = null;

	private bool _mouseOver = false;
	
	public override void _Ready()
	{
		#region Node fetching
		
		BgNode = GetNode<Polygon2D>("%Bg");
		FgNode = GetNode<Polygon2D>("%Fg");
		BBoxNode = GetNode<Control>("%BBox");
		CollisionPolyNode = GetNode<CollisionPolygon2D>("%CollisionPoly");
		CoordsLabelNode = GetNode<Label>("%CoordsLabel");
		
		#endregion
		
		_defaultBgColor = BgNode.Color;
	}

	public override void _Input(InputEvent e)
	{
		if (e.IsActionPressed("cancel-command"))
			Check();
	}

	public string CoordsStr => Coords.X + "." + Coords.Y;

	private string _playerID;
	public string PlayerID {
		get => _playerID;
		set {
			_playerID = value;
			if (string.IsNullOrEmpty(_playerID) || !_playerColors.ContainsKey(value)) return;

			var color = _playerColors[value];
			FgNode.Color = color;
		}
	}
	
	public void Check() {
		if (_processor is null) {
			return;
		}

		if (!_mouseOver) {
			Unfocus();
			return;
		}

		if (State is not null && State?.Entity is not null) {
			MatchCardState card = (MatchCardState)(State?.Entity);

			// TODO add back
			// _processor.HoverCard.Load(card);
		}
		
		if (!_processor.Accepts(this)) {
			Unfocus();
			return;
		} 
		BgNode.Color = HighlightColor;

		var command = _processor.CurrentCommand;
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

	public void Load(TileState? state) {
		State = state;
		if (state is null) {
			Visible = false;
			return;
		}

		Visible = true;
		PlayerID = state?.OwnerID;
	}
	
	// private void _pressed() {
	// 	if (!Client.Accepts(this)) return;

	// 	Client.Process(this);
	// 	// Map.MovementArrow.Visible = false;
	// 	Recheck();
	// }
	
	private void Unfocus() {
		BgNode.Color = _defaultBgColor;
	}

	public void SetPlayerColors(Dictionary<string, Color> colors)
	{
		_playerColors = colors;
		PlayerID = _playerID;
	}

	public void SetEntity(IEntity e)
	{
		Entity = e;
	}

	public IEntity GetEntity() => Entity;
	public Vector2 GetPosition() => Position;
	public void SetPosition(Vector2 pos) {
		Position = pos;
	}
	public Vector2 GetSize() => Size;
	public void SetCoords(Vector2 coords)
	{
		Coords = coords;
		CoordsLabelNode.Text = CoordsStr;
	}
	public Vector2 GetCoords() => Coords;
	public void SetShowId(bool v) {
		CoordsLabelNode.Visible = v;
	}

	public void SetCommandProcessor(CommandProcessor processor)
	{
		_processor = processor;
	}

	private void TryAddToAction() {
		if (!_processor.Accepts(this)) return;

		_processor.Process(this);
		// Map.MovementArrow.Visible = false;

		Check();
	}

	#region Signal connections

	private void OnCollisionMouseEntered()
	{
		_mouseOver = true;
		Check();
	}

	private void OnCollisionMouseExited()
	{
		_mouseOver = false;
		Unfocus();
		
		// SetColor(BaseColor);
		// if (HoverCard.Visible)
			// HoverCard.Visible = false;
		// if (Map.MovementArrow.Visible) Map.MovementArrow.Visible = false;
	}

	// private void _on_collision_input_event(Node viewport, InputEvent e, long shape_idx)
	// {
	// 	if (e.IsActionPressed("add-to-action"))
	// 		_pressed();
	// }

	private void OnCollisionInputEvent(Node viewport, InputEvent e, long shape_idx)
	{
		if (e.IsActionPressed("add-to-action")) {
			TryAddToAction();
		}
	}

    #endregion

    public TileState? GetState()
    {
		return State;
    }

    public string GetCoordsStr()
    {
		return CoordsStr;
    }
}
