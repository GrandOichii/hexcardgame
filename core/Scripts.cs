using core.match;
using System.Reflection;
using System.Linq.Expressions;
using util;
using NLua;
using core.cards;
using core.tiles;

namespace core.scripts;

/// <summary>
/// Marks the method as a Lua function
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
internal class LuaCommand : Attribute {}

/// <summary>
/// Script master of the match, creates all the utility function
/// </summary>
public class ScriptMaster {
    private Match _match;
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
    public int[] ParseCoords(LuaTable coords) {
        long? iPos = coords[1] as long?;
        if (iPos is null) {
            throw new Exception("Invalid point coordinates");
        }
        long? jPos = coords[2] as long?;
        if (jPos is null) {
            throw new Exception("Invalid point coordinates");
        }
        // TODO explicit cast - bad
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
            var point = pointRaw as LuaTable;
            if (point is null) {
                throw new Exception("Invalid point arguments for TileOwnerSet function");
            }
            var tile = TileAt(point);
            if (tile is null) {
                return;
            }


            // TODO for some reason pID is null, problem with setupScript
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
        var mCard = new MCard(_match, card, player);
        mCard.GoesToDiscard = false;

        var tile = TileAt(point);
        if (tile is null) {
            // TODO? throw exception
            return;
        }
        tile.Entity = mCard;

        player.AllCards.Add(mCard, Zones.PLACED);
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
        return null;
    }

    /// <summary>
    /// Deals damage to the entity at the specified tile
    /// </summary>
    /// <param name="dealerMID">Card match ID of the damage dealer</param>
    /// <param name="point">Tile coordinates</param>
    /// <param name="damage">Damage dealt</param>
    /// <returns>An array of amount of damage dealt and whether the target died</returns>
    [LuaCommand]
    public bool DealDamage(string dealerMID, LuaTable point, long damage) {
        var dealer = _match.GetCard(dealerMID);
        var tile = TileAt(point);
        if (tile is null) {
            return false;
        }

        var target = tile.Entity;
        if (target is null) {
            return false;
        }

        var dealt = target.ProcessDamage(damage);
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
                result.Add(null);
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
        var result = new List<object?>();

        var map = _match.Map;
        for (int i = 0; i < map.Height; i++) {
            for (int j = 0; j < map.Width; j++) {
                var tile = map.Tiles[i, j];
                if (tile is null) continue;
                var en = tile.Entity;
                if (en is null) continue;
                result.Add(en.Data);
            }
        }

        return LuaUtility.CreateTable(_match.LState, result);
    }
}