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
        var point = match.Map.TileAt(pointRaw);
        if (point is null) {
            // TODO? don't throw exception
            throw new Exception("Cannot play a card on point " + pointRaw + ": it is empty");
        }

        var card = player.Hand[mID];
        if (card is null) {
            // TODO? don't throw exception
            throw new Exception("Player " + player.ShortStr + " cannot play a card with mID" + mID + ": it is empty");
        }

        if (card.IsPlaceable) {
            // TODO
            // card is placeable, i.e. a Unit or a Structure
            // check whether destination tile is owned by the player
            // if not, cancel playing
            // remove the card from player's hand
            // place it onto the tile
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