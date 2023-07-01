using core.players;
using core.tiles;

namespace core.match;


/// <summary>
/// Game action, performed by the player
/// </summary>
abstract class GameAction
{
    /// <summary>
    /// Executes the game action
    /// </summary>
    /// <param name="match">Game match</param>
    /// <param name="player">Player who executes the action</param>
    /// <param name="args">The arguments of the action</param>
    abstract public void Exec(Match match, Player player, string[] args);
}


/// <summary>
/// Empty action, does nothing
/// </summary>
class DoNothingAction : GameAction
{
    public override void Exec(Match match, Player player, string[] args)
    {
    }
}

/// <summary>
/// Action for executing match Lua commands
/// </summary>
class ExecuteCommandAction : GameAction
{
    public override void Exec(Match match, Player player, string[] args)
    {
        if (!match.AllowCommands) return;

        var command = "";
        for (int i = 1; i < args.Length; i++)
            command += args[i] + " ";
        match.LState.DoString(command);
    }
}


class PlayCardAction : GameAction
{
    public override void Exec(Match match, Player player, string[] args)
    {
        // args[1] - the MID of the card
        // args[2] - the tile the card is played on
        if (args.Length != 3) {
            throw new Exception("Incorrect number of arguments for play action");
        }

        var mID = args[1];
        var pointRaw = args[2];
        var tile = match.Map.TileAt(pointRaw);
        if (tile is null) {
            // TODO? don't throw exception
            throw new Exception("Cannot play a card on point " + pointRaw + ": it is empty");
        }

        var card = player.Hand[mID];
        if (card is null) {
            // TODO? don't throw exception
            throw new Exception("Player " + player.ShortStr + " cannot play a card with mID" + mID + ": it is empty");
        }

        if (card.IsPlaceable) {
            if (tile.Owner != player) {
                // TODO? don't throw exception
                throw new Exception("Can't place entity on tile " + pointRaw + ": it's not owned by " + player.ShortStr);
            }
            if (tile.Entity is object) {
                // TODO? don't throw exception 
                throw new Exception("Can't place entity on tile " + pointRaw + ": it is already taken");
            }
            player.Hand.Cards.Remove(card);
            tile.Entity = card;
            return;
        }

        // card is not placeable, i.e. is a Spell
        // check, whether the specified tile has a Mage Unit, owned by the player
        // if not, cancel playing
        // check, if all targets are chosen
        // if not, cancel playing
        // remove card from hand, place it into discard, resolve it's effects
    }
}


class MoveAction : GameAction
{
    public override void Exec(Match match, Player player, string[] args)
    {
        // move 2.1 0
        if (args.Length != 3) {
            throw new Exception("Incorrect number of arguments for move action");
        }

        // args[1] - point
        // args[2] - direction

        var tile = match.Map.TileAt(args[1]);

        // TODO better errors
        if (tile is null) {
            // TODO? don't throw exception
            throw new Exception("Invalid point argument for move action");
        }
        if (tile.Entity is null) {
            // TODO? don't throw exception
            throw new Exception("Invalid point argument for move action");
        }
        if (tile.Entity.Owner != player) {
            // TODO? don't throw exception
            throw new Exception("Invalid point argument for move action");
        }
        if (!tile.Entity.IsUnit) {
            // TODO? don't throw exception
            throw new Exception("Invalid point argument for move action");
        }
        // TODO movement
        
        var en = tile.Entity;
        var dir = int.Parse(args[2]);
        var newTile = match.Map.GetNeighbor(tile.IPos, tile.JPos, dir);

        if (newTile is null) {
            // TODO? don't throw exception
            throw new Exception("Can't move to tile with args: " + args[1] + " " + args[2] + ": it is empty");
        }

        tile.Entity = null;
        newTile.Entity = en;
        en.Data["movement"] = en.Movement - 1;
    }
}