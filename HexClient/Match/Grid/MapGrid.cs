using Godot;
using HexCore.GameMatch.States;
using System;
using System.Collections.Generic;

namespace HexClient.Match.Grid;

public interface ITile : IGamePart {
	public void SetPlayerColors(Dictionary<string, Color> colors);
	public void Load(TileState? state);
	public void SetEntity(IEntity e);
	public IEntity GetEntity();
	public Vector2 GetPosition();
	public void SetPosition(Vector2 pos);
	public Vector2 GetSize();
	public void SetCoords(Vector2 coords);
	public void SetShowId(bool v);
	public void SetCommandProcessor(CommandProcessor processor);
}

public partial class MapGrid : Control, IMapGrid
{
	#region Packed scenes

	[Export]
	private PackedScene TilePS { get; set; }
	[Export]
	private PackedScene EntityPS { get; set; }

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

	private List<List<ITile>> _tiles = null;
	private BaseState _state;
	private Dictionary<string, MatchCardState> _entities = new();
	private Dictionary<string, int[]> _enPositions = new();
	// private MatchConnection _client;
	// public MatchConnection Client {
	// 	get => _client; 
	// 	set {
	// 		_client = value;
			
	// 	}
	// }

	private bool _showEntityIds = false;
	private CommandProcessor? _processor = null;

	private Dictionary<string, Color> _playerColors = new Dictionary<string, Color>() {
		{ "1", new Color(1, 0, 0) },
		{ "2", new Color(1, 1, 0) },
	};

	public void SetPlayerColors(Dictionary<string, Color> colors) {
		_playerColors = colors;
		if (_tiles is null) return;

		foreach (var line in _tiles) {
			foreach (var tile in line) {
				tile.SetPlayerColors(colors);
				tile.GetEntity()?.SetPlayerColors(colors);
			}
		}
	}

	public override void _Ready()
	{
		#region Node fetching
		
		TilesNode = GetNode<Node2D>("%Tiles");
		EntitiesNode = GetNode<Node2D>("%Entities");
		
		#endregion
	}
	
	public void Load(BaseState state) {
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
		var newEn = new Dictionary<string, MatchCardState>();
		var newPos = new Dictionary<string, int[]>();
		for (int i = 0; i < tiles.Count; i++) {
			var row = tiles[i];
			for (int j = 0; j < row.Count; j++) {
				var tile = row[j];
				if (tile is null || tile?.Entity is null) continue;
				var en = (MatchCardState)(tile?.Entity);
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
			t.SetEntity(null);

			EntitiesNode.RemoveChild(child);
			child.Free();
		}

		// add new entities
		foreach (var pair in addedEntities) {
			var mid = pair.Key;
			var entity = pair.Value;
			var child = EntityPS.Instantiate() as Entity;

			EntitiesNode.AddChild(child);

			var eNode = child as IEntity;
			eNode.SetShowId(_showEntityIds);
			eNode.SetPlayerColors(_playerColors);
			eNode.Load(entity);

			var loc = newPos[mid];
			var t = _tiles[loc[0]][loc[1]];
			eNode.SetPosition(new(t.GetPosition().X, t.GetPosition().Y));
			t.SetEntity(eNode);
		}

		// move existing entities
		foreach (var mid in sameKeys) {
			var newP = newPos[mid];
			var oldP = _enPositions[mid];
			var t = _tiles[oldP[0]][oldP[1]];
			var e = t.GetEntity();
			if (!(newP[0] == oldP[0] && newP[1] == oldP[1])) {
				t.SetEntity(null);
				var newT = _tiles[newP[0]][newP[1]];
				newT.SetEntity(e);

				e.TweenPosition(newT.GetPosition(), .1);
			}
			e.Load(newEn[mid]);
		}

		_entities = newEn;
		_enPositions = newPos;
	}

	public void SetCommandProcessor(CommandProcessor processor)
	{
		_processor = processor;
		if (_tiles is null) return;

		foreach (var line in _tiles) {
			foreach (var tile in line) {
				tile.SetCommandProcessor(processor);
			}
		}
	}

	private void PopulateTiles() {
		var state = _state.Map;
		_tiles = new();
		for (int i = 0; i < state.Tiles.Count; i++) {
			var a = new List<ITile>();
			for (int j = 0; j < state.Tiles[i].Count; j++) {
				var child = TilePS.Instantiate() as Tile;
				TilesNode.AddChild(child);

				var tile = child as ITile;
				tile.SetCommandProcessor(_processor);
				tile.SetPlayerColors(_playerColors);
				
				a.Add(tile);
			}
			_tiles.Add(a);
		}
	}

	#region Utility static methods

	private static List<string> SameKeys(Dictionary<string, MatchCardState> first, Dictionary<string, MatchCardState> second) {
		var result = new List<string>();
		foreach (var key in first.Keys)
			if (second.ContainsKey(key)) result.Add(key);
		return result;
	}
	
	private static Dictionary<string, MatchCardState> Difference(Dictionary<string, MatchCardState> first, Dictionary<string, MatchCardState> second) {
		var result = new Dictionary<string, MatchCardState>();
		foreach (var key in first.Keys) {
			if (second.ContainsKey(key)) continue;
			result.Add(key, first[key]);
		}
		return result;
	}

	#endregion
	
	#region Signal connections
	
	private void OnResized()
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
					var size = tile.GetSize();
					tileHeight = size.Y;
					tileWidth = size.X;
				}
				var y = (tileHeight) * .5f * i;
				var b = (tileWidth+3) * 3 / 4;
				var x = b * 2 * j + (1 - i % 2) * b;
				tile.SetPosition(new(x, y));
				tile.SetCoords(new(i, j));
			
				if (y > maxY) maxY = y;
				if (x > maxX) maxX = x;
			}
		}
		CustomMinimumSize = new(maxX + 2 * XMinOffset, maxY + 2 * YMinOffset);
		var xDiff = (Size.X - maxX) / 2;
		var yDiff = (Size.Y - maxY) / 2;
		foreach (var row in _tiles) {
			foreach (var tile in row) {
				var x = tile.GetPosition().X + xDiff;
				var y = tile.GetPosition().Y + yDiff;
				tile.SetPosition(new(x, y));
				tile.GetEntity()?.SetPosition(new(x, y));
			}
		}
	}
	
	private void OnMatchShowTileIdsToggled(bool v) {
		foreach (var line in _tiles) {
			foreach (var tile in line) {
				tile.SetShowId(v);
			}
		}
	}

	private void OnMatchShowEntityIdsToggled(bool v)
	{
		_showEntityIds = v;
		foreach (var line in _tiles) {
			foreach (var tile in line) {
				tile.GetEntity()?.SetShowId(v);
			}
		}
	}

	#endregion
}


