using core.cards;
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
    private void LogPlayed(Match match, Player player, MCard card) {
        match.Logger.ParseAndLog(player.Name + " played " + card.ToLogForm + ".");
    }

    public override void Exec(Match match, Player player, string[] args)
    {
        // args[1] - the MID of the card
        // args[2] - the tile the card is played on
        if (args.Length != 3) {
            if (!match.StrictMode) return;
            throw new Exception("Incorrect number of arguments for play action");
        }

        var mID = args[1];
        var pointRaw = args[2];
        var tile = match.Map.TileAt(pointRaw);
        if (tile is null) {
            // TODO? don't throw exception
            if (!match.StrictMode) return;
            throw new Exception("Cannot play a card on point " + pointRaw + ": it is empty");
        }

        var card = player.Hand[mID];
        if (card is null) {
            // TODO? don't throw exception
            if (!match.StrictMode) return;
            throw new Exception("Player " + player.ShortStr + " cannot play a card with mID " + mID + ": they don't have it in their hand");
        }

        if (card.IsPlaceable) {
            if (tile.Owner != player) {
                // TODO? don't throw exception
                if (!match.StrictMode) return;
                throw new Exception("Can't place entity on tile " + pointRaw + ": it's not owned by " + player.ShortStr);
            }
            if (tile.Entity is object) {
                // TODO? don't throw exception
                if (!match.StrictMode) return; 
                throw new Exception("Can't place entity on tile " + pointRaw + ": it is already taken");
            }
            if (!player.TryPlayCard(card)) {
                // failed to play card
                return;
            }

            player.Hand.Cards.Remove(card);
            LogPlayed(match, player, card);
            tile.Entity = card;
            player.AllCards[card] = Zones.PLACED;

            // call OnEnter function
            card.ExecFunc("OnEnter", card.Data, player.ID, tile.ToLuaTable(match.LState));
            
            return;
        }

        var caster = tile.Entity;
        if (caster is null) {
            // TODO? don't throw exception
            if (!match.StrictMode) return;
            throw new Exception("Failed to cast spell " + card.ShortStr + ": tile at " + args[2] + " has no entity");
        }
        if (caster.Owner != player) {
            // TODO? don't throw exception
            if (!match.StrictMode) return;
            throw new Exception("Failed to cast spell " + card.ShortStr + ": entity at " + args[2] + " is not owned by the player who played the spell");
        }
        if (!caster.Type.Contains("Mage")) {
            // TODO? don't throw exception
            if (!match.StrictMode) return;
            throw new Exception("Failed to cast spell " + card.ShortStr + ": entity at " + args[2] + " is not a Mage");
        }
        // TODO modify spell here

        if (!player.TryPlayCard(card)) {
            // failed to play card
            return;
        }

        player.Hand.Cards.Remove(card); 
        player.AllCards[card] = Zones.PLAYED;
        // emit the played signal
        match.Emit("spell_cast", new(){ {"casterID", caster.MID}, { "spellID", card.MID } });
        LogPlayed(match, player, card);

        // execute the effect of the card
        card.ExecFunc(MCard.EFFECT_FNAME, card.Data, player.ID, caster.Data);
        player.AllCards[card] = Zones.DISCARD;
        player.Discard.AddToBack(card);

        // TODO demodify spell here
    }
}


class MoveAction : GameAction
{
    public override void Exec(Match match, Player player, string[] args)
    {
        // move 2.1 0
        if (args.Length != 3) {
            if (!match.StrictMode) return;
            throw new Exception("Incorrect number of arguments for move action");
        }

        // args[1] - point
        // args[2] - direction

        var tile = match.Map.TileAt(args[1]);

        // TODO better errors
        if (tile is null) {
            // TODO? don't throw exception
            if (!match.StrictMode) return;
            throw new Exception("Invalid point argument for move action: tile is null");
        }
        var en = tile.Entity;
        if (en is null) {
            // TODO? don't throw exception
            if (!match.StrictMode) return;
            throw new Exception("Invalid point argument for move action: tile has no entity to be moved");
        }
        if (en.Owner != player) {
            // TODO? don't throw exception
            if (!match.StrictMode) return;
            throw new Exception("Invalid point argument for move action: entity is not owned by the player");
        }
        if (!en.IsUnit) {
            // TODO? don't throw exception
            if (!match.StrictMode) return;
            throw new Exception("Invalid point argument for move action: entity is not a Unit");
        }
        // TODO movement
        if (!en.CanMove) {
            // TODO? don't throw exception
            if (!match.StrictMode) return;
            throw new Exception("Invalid point argument for move action: unit has no movement points left");
        }
        
        var dir = int.Parse(args[2]);
        var newTile = match.Map.GetNeighbor(tile.IPos, tile.JPos, dir);

        if (newTile is null) {
            // TODO? don't throw exception
            if (!match.StrictMode) return;
            throw new Exception("Can't move to tile with args: " + args[1] + " " + args[2] + ": it is empty");
        }

        var targetEn = newTile.Entity;

        // tried to move onto a tile that has an entity in it
        if (targetEn is not null) {
            var owner = targetEn.Owner;
            if (owner == player) {
                // TODO? don't throw exception
                if (!match.StrictMode) return;
                throw new Exception("Can't move to tile with args: " + args[1] + " " + args[2] + ": it already has an entity that is owned by the same player.");
            }
            match.SystemLogger.Log("MOVEACTION", en.ShortStr + " attacks " + targetEn.ShortStr);

            en.Data["movement"] = en.Movement - 1;

            // tried to attack
            var attackerDamage = en.Power;
            var defenderDamage = targetEn.Power;

            // deal damage
            if (attackerDamage > 0) {
                targetEn.ProcessDamage(attackerDamage);
            }

            if (defenderDamage > 0) {
                en.ProcessDamage(defenderDamage);
            }

            match.Logger.ParseAndLog(player.Name + "'s " + en.ToLogForm + " attacks " + targetEn.ToLogForm + ".");

            match.CheckZeroLife();
            return;
        }


        tile.Entity = null;
        newTile.Entity = en;
        en.Data["movement"] = en.Movement - 1;
        match.Logger.ParseAndLog(player.Name + " moved " + en.ToLogForm + ".");

        match.Emit("unit_move", new(){ {"mid", en.MID}, {"tile", newTile.ToLuaTable(match.LState)} });
    }
}