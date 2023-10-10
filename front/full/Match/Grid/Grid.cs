using core.match.states;
using Godot;
using System;
using System.Collections.Generic;

public partial class Grid : Control
{
	#region Packed scenes

	private readonly static PackedScene TilePS = ResourceLoader.Load<PackedScene>("res://Match/Grid/Tile.tscn");
	private readonly static PackedScene EntityPS = ResourceLoader.Load<PackedScene>("res://Match/Grid/Entity.tscn");

	#endregion
	
	#region Exports
	
	[Export]
	public float XMinOffset;
	[Export]
	public float YMinOffset;
	
	#endregion

	#region Nodes
	
	public Node2D TilesNode { get; private set; }
	public Node2D EntitiesNode { get; private set; }
	
	#endregion

	private List<List<Tile>> _tiles = null;
	private MatchState _state;
	private Dictionary<string, MCardState> _entities = new();
	private Dictionary<string, int[]> _enPositions = new();
	private MatchConnection _client;
	public MatchConnection Client {
		get => _client; 
		set {
			_client = value;
			
		}
	}


	public override void _Ready()
	{
		#region Node fetching
		
		TilesNode = GetNode<Node2D>("%Tiles");
		EntitiesNode = GetNode<Node2D>("%Entities");
		
		#endregion
	}
	
	public void Load(MatchState state) {
		_state = state;
		if (_tiles is null) {
			PopulateTiles();
		}

		// load tile data
		for (int i = 0; i < state.Map.Tiles.Count; i++) {
			for (int j = 0; j < state.Map.Tiles[i].Count; j++) {
				_tiles[i][j].Load(state.Map.Tiles[i][j]);
			}
		}

		var tiles = state.Map.Tiles;
		var newEn = new Dictionary<string, MCardState>();
		var newPos = new Dictionary<string, int[]>();
		for (int i = 0; i < tiles.Count; i++) {
			var row = tiles[i];
			for (int j = 0; j < row.Count; j++) {
				var tile = row[j];
				if (tile is null || tile?.Entity is null) continue;
				var en = (MCardState)(tile?.Entity);
				newEn.Add(en.MID, en);
				newPos.Add(en.MID, new int[]{i, j});
			}
		}
		
		var removedEntities = Difference(_entities, newEn);
		var addedEntities = Difference(newEn, _entities);
		var sameKeys = SameKeys(newEn, _entities);
		
		// remove old entities
		for (int i = EntitiesNode.GetChildCount() - 1; i >= 0; i--) {
			var child = EntitiesNode.GetChild<Entity>(i);
			var mid = child.State.MID;
			if (!removedEntities.ContainsKey(mid)) continue;

			var pos = _enPositions[mid];
			var t = _tiles[pos[0]][pos[1]];
			t.Entity = null;

			EntitiesNode.RemoveChild(child);
			child.Free();
		}

		// add new entities
		foreach (var pair in addedEntities) {
			var mid = pair.Key;
			var entity = pair.Value;
			var eNode = EntityPS.Instantiate() as Entity;
			EntitiesNode.AddChild(eNode);
			eNode.Load(entity);

			var loc = newPos[mid];
			var t = _tiles[loc[0]][loc[1]];
			// eNode.Position = t.GlobalPosition;
			eNode.Position = new(t.Position.X, t.Position.Y);
			t.Entity = eNode;
		}

		// move existing entities
		foreach (var mid in sameKeys) {
			var newP = newPos[mid];
			var oldP = _enPositions[mid];
			var t = _tiles[oldP[0]][oldP[1]];
			var e = t.Entity;
			if (!(newP[0] == oldP[0] && newP[1] == oldP[1])) {
				t.Entity = null;
				var newT = _tiles[newP[0]][newP[1]];
				newT.Entity = e;
				e.CreateTween().TweenProperty(e, "position", newT.Position, .1);
			}
			e.Load(newEn[mid]);
		}

		_entities = newEn;
		_enPositions = newPos;
	}

	private void PopulateTiles() {
		var state = _state.Map;
		_tiles = new();
		for (int i = 0; i < state.Tiles.Count; i++) {
			var a = new List<Tile>();
			for (int j = 0; j < state.Tiles[i].Count; j++) {
				var tile = TilePS.Instantiate() as Tile;
				TilesNode.AddChild(tile);
				tile.Client = Client;
				// tile.Map = this;
				
				a.Add(tile);
			}
			_tiles.Add(a);
		}
	}

	#region Utility static methods

	private static List<string> SameKeys(Dictionary<string, MCardState> first, Dictionary<string, MCardState> second) {
		var result = new List<string>();
		foreach (var key in first.Keys)
			if (second.ContainsKey(key)) result.Add(key);
		return result;
	}
	
	private static Dictionary<string, MCardState> Difference(Dictionary<string, MCardState> first, Dictionary<string, MCardState> second) {
		var result = new Dictionary<string, MCardState>();
		foreach (var key in first.Keys) {
			if (second.ContainsKey(key)) continue;
			result.Add(key, first[key]);
		}
		return result;
	}

	#endregion
	
	#region Signal connections
	
	private void _on_resized()
	{
		float maxY = 0;
		float maxX = 0;
		float tileHeight = 0;
		float tileWidth = 0;
		if (_tiles is null) return;
		for (int i = 0; i < _tiles.Count; i++) {
			var row = _tiles[i];
			for (int j = 0; j < row.Count; j++) {
				var tile = _tiles[i][j];
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
			
				if (y > maxY) maxY = y;
				if (x > maxX) maxX = x;
			}
		}
		CustomMinimumSize = new(maxX + 2 * XMinOffset, maxY + 2 * YMinOffset);
		var xDiff = (Size.X - maxX) / 2;
		var yDiff = (Size.Y - maxY) / 2;
		foreach (var row in _tiles) {
			foreach (var tile in row) {
				var x = tile.Position.X + xDiff;
				var y = tile.Position.Y + yDiff;
				tile.Position = new(x, y);
				if (tile.Entity is not null) tile.Entity.Position = new(x, y);
				
			}
		}
	}
	
	#endregion
}


