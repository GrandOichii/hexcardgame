using core.match.states;
using Godot;
using System;

public partial class Tile : Node2D
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
	
	public override void _Ready()
	{
		#region Node fetching
		
		BgNode = GetNode<Polygon2D>("%Bg");
		FgNode = GetNode<Polygon2D>("%Fg");
		BBoxNode = GetNode<Control>("%BBox");
		CollisionPolyNode = GetNode<CollisionPolygon2D>("%CollisionPoly");
		
		#endregion
		
		_defaultBgColor = BgNode.Color;

//		var poly = CollisionPolyNode.Polygon;
//		BgNode.Polygon = poly;
//		FgNode.Polygon = poly;
//		FgNode.Scale = new(.9f, .9f);
	}
	
	public void Load(TileState? state) {
		State = state;
		if (state is null) {
			Visible = false;
			return;
		}

		Visible = true;
	}
	
	#region Signal connections

	private void _on_collision_mouse_entered()
	{
		BgNode.Color = HighlightColor;
	}

	private void _on_collision_mouse_exited()
	{
		BgNode.Color = _defaultBgColor;
	}
	
	#endregion
}

