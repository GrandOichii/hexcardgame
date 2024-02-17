using System.Linq.Expressions;
using System.Reflection;
using NLua;
using Util;

namespace Core.GameMatch.Scripts;

/// <summary>
/// Script master of the match, creates all the utility function
/// </summary>
public class ScriptMaster {
    private readonly Match _match;
    public ScriptMaster(Match match) {
        _match = match;

        // load all methods into the Lua stat
        var type = typeof(ScriptMaster);
        foreach (var method in type.GetMethods())
        {
            if (method.GetCustomAttribute(typeof(LuaCommand)) is not null)
            {
                _match.LState[method.Name] = method.CreateDelegate(Expression.GetDelegateType(
                    (from parameter in method.GetParameters() select parameter.ParameterType)
                    .Concat(new[] { method.ReturnType })
                .ToArray()), this);
            }
        }
    }

    /// <summary>
    /// Parses a Lua array into an array of coordinates
    /// </summary>
    /// <param name="coords">Lua table of coordinates</param>
    /// <returns>An integer array of coordinates</returns>
    static public int[] ParseCoords(LuaTable coords) {
        long? iPos = coords[1] as long? ?? throw new Exception("Invalid point coordinates");
        long? jPos = coords[2] as long? ?? throw new Exception("Invalid point coordinates");
        return new int[] {(int)iPos, (int)jPos};
    }

    /// <summary>
    /// Returns the tile at the specified coordinates
    /// </summary>
    /// <param name="coords">Coordinates in the form of a Lua table</param>
    /// <returns>Tile</returns>
    private Tile? TileAt(LuaTable coords) {
        var actualCoords = ParseCoords(coords);
        var tile = _match.Map.Tiles[actualCoords[0], actualCoords[1]];
        return tile;
    }

    /// <summary>
    /// Returns a list of all entities on board that are either of the specified types
    /// </summary>
    /// <param name="types">Types of entities</param>
    /// <returns>List of entities</returns>
    private LuaTable GetOnBoard(params string[] types) {
        var result = new List<object?>();

        var map = _match.Map;
        for (int i = 0; i < map.Height; i++) {
            for (int j = 0; j < map.Width; j++) {
                var tile = map.Tiles[i, j];
                if (tile is null) continue;
                var en = tile.Entity;
                if (en is null) continue;
                if (!types.Contains(en.BaseType)) continue;
                result.Add(tile.ToLuaTable(_match.LState));
            }
        }

        return LuaUtility.CreateTable(_match.LState, result);
    }

    /// <summary>
    /// Log wrapper for system log
    /// </summary>
    /// <param name="message">Log message</param>
    [LuaCommand]
    public void Log(string message) {
        _match.SystemLogger.Log("SCRIPTS", message);
    }

    /// <summary>
    /// Returns short string for player
    /// </summary>
    /// <param name="pID">Player ID</param>
    /// <returns>Short string</returns>
    [LuaCommand]
    public string PlayerShortStr(string pID) {
        var player = _match.PlayerWithID(pID);
        return player.ShortStr;
    }

    /// <summary>
    /// Returns a short string for card
    /// </summary>
    /// <param name="mID">Card match ID</param>
    /// <returns>Short string</returns>
    [LuaCommand]
    public string CardShortStr(string mID) {
        var card = _match.GetCard(mID);
        return card.ShortStr;
    }

    /// <summary>
    /// Returns a Lua array with all the player IDs
    /// </summary>
    /// <returns>Lua array of player IDs</returns>
    [LuaCommand]
    public LuaTable GetPlayerIDs() {
        var result = new List<string>();
        foreach (var player in _match.Players)
            result.Add(player.ID);
        return LuaUtility.CreateTable(_match.LState, result);
    }
    
    /// <summary>
    /// Sets the owner of the tiles to the specified player id
    /// </summary>
    /// <param name="points">List of points</param>
    /// <param name="pID">Player ID of the new owner of the tiles</param>
    [LuaCommand]
    public void TileOwnerSet(string pID, LuaTable points) {
        foreach (var pointRaw in points.Values) {
            var point = pointRaw as LuaTable ?? throw new Exception("Invalid point arguments for TileOwnerSet function");
            var tile = TileAt(point);
            if (tile is null) {
                return;
            }

            var player = _match.PlayerWithID(pID);

            tile.Owner = player;
        }
    }

    /// <summary>
    /// Returns a lua table of player, not containing any card info
    /// </summary>
    /// <param name="pID">Player ID</param>
    /// <returns>Lua table</returns>
    [LuaCommand]
    public LuaTable GetShortInfo(string pID) {
        var player = _match.PlayerWithID(pID);
        var result = LuaUtility.CreateTable(_match.LState);

        result["name"] = player.Name;
        result["id"] = player.ID;
        result["energy"] = player.Energy;
        
        return result;
    }

    /// <summary>
    /// Subtracts the amount from player's energy
    /// </summary>
    /// <param name="pID">Player ID</param>
    /// <param name="amount">Amount of energy</param>
    [LuaCommand]
    public void PayEnergy(string pID, int amount) {
        var player = _match.PlayerWithID(pID);
        player.Energy -= amount;
    }

    /// <summary>
    /// Creates and puts the entity on the specified tile
    /// </summary>
    /// <param name="pID">Owner ID</param>
    /// <param name="point">Tile coordinates</param>
    /// <param name="cID">ID of the card</param>
    [LuaCommand]
    public void CreateAndPutEntity(string pID, LuaTable point, string cID) {
        var player = _match.PlayerWithID(pID);
        var card = _match.CardMaster.Get(cID);
        var mCard = new MCard(_match, card, player)
        {
            GoesToDiscard = false
        };

        var tile = TileAt(point);
        if (tile is null) {
            // TODO? throw exception
            return;
        }
        tile.Entity = mCard;

        player.AllCards.Add(mCard, ZoneTypes.PLACED);
    }

    /// <summary>
    /// Adds the amount to the player's energy reserve
    /// </summary>
    /// <param name="pID">ID of the player</param>
    /// <param name="amount">Amount of energy</param>
    [LuaCommand]
    public void AddEnergy(string pID, int amount) {
        var player = _match.PlayerWithID(pID);
        player.Energy += amount;
    }

    /// <summary>
    /// Returns the ID of the card owner
    /// </summary>
    /// <param name="mID">Card match ID</param>
    /// <returns>ID of the owner</returns>
    [LuaCommand]
    public string GetOwnerID(string mID) {
        return _match.GetCard(mID).Owner.ID;
    }

    /// <summary>
    /// Finds and returns the tile info with the specified entity
    /// </summary>
    /// <param name="mID">Card match ID</param>
    /// <returns>Tile info</returns>
    [LuaCommand]
    public LuaTable? GetTileWith(string mID) {
        var map = _match.Map;
        for (int i = 0; i < map.Height; i++) {
            for (int j = 0; j < map.Width; j++) {
                var tile = map.Tiles[i, j];
                if (tile is null) continue;
                var en = tile.Entity;
                if (en is null) continue;
                if (en.MID == mID) return tile.ToLuaTable(_match.LState);
            }
        }
        throw new Exception("Failed to locate entity on board with ID " + mID);
    }

    /// <summary>
    /// Deals damage to the entity at the specified tile
    /// </summary>
    /// <param name="dealerMID">Card match ID of the damage dealer</param>
    /// <param name="point">Tile coordinates</param>
    /// <param name="damage">Damage dealt</param>
    /// <returns>An array of amount of damage dealt and whether the target died</returns>
    [LuaCommand]
    public bool DealDamage(string dealerID, LuaTable point, long damage) {
        var dealer = _match.GetCard(dealerID);
        var tile = TileAt(point);
        if (tile is null) {
            return false;
        }

        var target = tile.Entity;
        if (target is null) {
            return false;
        }

        var dealt = target.ProcessDamage(dealerID, damage);
        // TODO add more logging
        _match.Logger.ParseAndLog("Card " + dealer.ToLogForm + " dealt " + dealt + " damage to " + target.ToLogForm + ".");
        var died = target.Life == 0;

        _match.CheckZeroLife();

        return died;
    }

    /// <summary>
    /// Forces the specified player to draw cards
    /// </summary>
    /// <param name="pID">Player ID</param>
    /// <param name="amount">Amount of cards to be drawn</param>
    /// <returns>The amount of cards the player drew</returns>
    [LuaCommand]
    public int DrawCards(string pID, int amount) {
        var player = _match.PlayerWithID(pID);
        return player.Draw(amount);
    }

    /// <summary>
    /// Returns a Lua table of all neighboring tiles
    /// </summary>
    /// <param name="point">Coordinates of the center tile</param>
    /// <returns>A lua table of tiles</returns>
    [LuaCommand]
    public LuaTable GetNeighbors(LuaTable point) {
        var coords = ParseCoords(point);
        var result = new List<object?>();

        for (int i = 0; i < 6; i++) {
            var n = _match.Map.GetNeighbor(coords[0], coords[1], i);
            if (n is null) {
                // result.Add(null);
                continue;
            }
            result.Add(n.ToLuaTable(_match.LState));
        }

        return LuaUtility.CreateTable(_match.LState, result);
    }

    /// <summary>
    /// Returns a Lua array wuth all of the Units that take a tile
    /// </summary>
    /// <returns>A Lua array</returns>
    [LuaCommand]
    public LuaTable GetUnitsOnBoard() {
        return GetOnBoard("Unit");
    }

    /// <summary>
    /// Returns a Lua array wuth all of the Structures that take a tile
    /// </summary>
    /// <returns>A Lua array</returns>
    [LuaCommand]
    public LuaTable GetStructuresOnBoard() {
        return GetOnBoard("Structure");
    }

    /// <summary>
    /// Returns a Lua array wuth all of the Units and Structures that take a tile
    /// </summary>
    /// <returns>A Lua array</returns>
    [LuaCommand]
    public LuaTable GetUnitsAndStructuresOnBoard() {
        return GetOnBoard("Structure", "Unit");
    }

    /// <summary>
    /// Summons a new card
    /// </summary>
    /// <param name="fromMID">Card source of the summoning</param>
    /// <param name="ownerID">The owner of the new card</param>
    /// <param name="cID">The card id</param>
    /// <returns>The summoned card's data</returns>
    [LuaCommand]
    public LuaTable SummonCard(string fromMID, string ownerID, string cID) {
        // TODO? fromMID not utilized yet
        var player = _match.PlayerWithID(ownerID);
        var fromC = _match.GetCard(fromMID);
        var card = _match.CardMaster.Get(cID);

        var newCard = new MCard(_match, card, player)
        {
            GoesToDiscard = false
        };
        player.AllCards.Add(newCard, "");
        return newCard.Data;
    }

    /// <summary>
    /// Adds the specified card to the player's hand. !Does not remove from previous zone.
    /// </summary>
    /// <param name="pID">Player ID</param>
    /// <param name="mID">Card match ID</param>
    [LuaCommand]
    public void PlaceCardInHand(string pID, string mID) {
        var player = _match.PlayerWithID(pID);
        var card = _match.GetCard(mID);
        player.AllCards[card] = ZoneTypes.HAND;
        player.Hand.AddToBack(card);
    }

    /// <summary>
    /// Returns card data by card match ID
    /// </summary>
    /// <param name="mID">Card match ID</param>
    /// <returns>Card data</returns>
    [LuaCommand]
    public LuaTable GetCard(string mID) {
        return _match.GetCard(mID).Data;
    }

    // TODO add docs
    [LuaCommand]
    public LuaTable? PickTile(string pID, string mID, LuaTable choices) {
        var player = _match.PlayerWithID(pID);
        var card = _match.GetCard(mID);
        var c = new List<int[]>();
        foreach (LuaTable rawP in choices.Values) {
            var point = ParseCoords(rawP);
            c.Add(point);
        }
        var result = player.Controller.PickTile(c, player, _match);
        var tile = _match.Map.TileAt(result);
        if (tile is null) return null;
        return tile.ToLuaTable(_match.LState);
    }
}