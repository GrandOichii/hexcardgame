using Godot;
using System;

using core.match.states;
using System.Collections.Generic;

public partial class Map : Panel
{

	static readonly PackedScene TileBaseScene = ResourceLoader.Load("res://Match/Tiles/TileBase.tscn") as PackedScene;
	public List<List<TileBase>> Tiles { get; private set; } = null;
	
	public Polygon2D MovementArrow { get; private set; }
	private Node2D TilesNode;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MovementArrow = GetNode<Polygon2D>("MovementArrow");
		TilesNode = GetNode<Node2D>("Tiles");
	}
	
	public void Load(MapState state) {
		if (Tiles is null) {
			Tiles = new();
			float tileHeight = 0;
			float tileWidth = 0;
			for (int i = 0; i < state.Tiles.Count; i++) {
				var a = new List<TileBase>();
				for (int j = 0; j < state.Tiles[i].Count; j++) {
					var tile = TileBaseScene.Instantiate() as TileBase;
					TilesNode.AddChild(tile);
					tile.Map = this;
					if (tileHeight == 0) {
						var size = tile.Size;
						tileHeight = size.Y;
						tileWidth = size.X;
					}

					var y = (tileHeight) * .5f * i;
					var b = (tileWidth+3) * 3 / 4;
					var x = b * 2 * j + (1 - i % 2) * b;
					tile.Position = new(x, y);
					tile.Coords = new(j, i);
					a.Add(tile);
				}
				Tiles.Add(a);
			}
		}


		// load the info
		for (int i = 0; i < state.Tiles.Count; i++) {
			for (int j = 0; j < state.Tiles[i].Count; j++) {
				Tiles[i][j].Load(state.Tiles[i][j]);
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
