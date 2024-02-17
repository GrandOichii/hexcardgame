namespace Core.GameMatch.Actions;


/// <summary>
/// Action for moving a Unit
/// </summary>
class MoveAction : IGameAction
{
    public void Exec(Match match, Player player, string[] args)
    {
        // move 2.1 0
        if (args.Length != 3) {
            if (!match.StrictMode) return;
            throw new Exception("Incorrect number of arguments for move action");
        }

        // args[1] - point
        // args[2] - direction

        var tile = match.Map.TileAt(args[1]);

        if (tile is null) {
            if (!match.StrictMode) return;
            throw new Exception("Invalid point argument for move action: tile is null");
        }
        var en = tile.Entity;
        if (en is null) {
            if (!match.StrictMode) return;
            throw new Exception("Invalid point argument for move action: tile has no entity to be moved");
        }
        if (en.Owner != player) {
            if (!match.StrictMode) return;
            throw new Exception("Invalid point argument for move action: entity is not owned by the player");
        }
        if (!en.IsUnit) {
            if (!match.StrictMode) return;
            throw new Exception("Invalid point argument for move action: entity is not a Unit");
        }
        if (!en.CanMove) {
            if (!match.StrictMode) return;
            throw new Exception("Invalid point argument for move action: unit has no movement points left");
        }
        
        var dir = int.Parse(args[2]);
        var newTile = match.Map.GetNeighbor(tile.IPos, tile.JPos, dir);

        if (newTile is null) {
            if (!match.StrictMode) return;
            throw new Exception("Can't move to tile with args: " + args[1] + " " + args[2] + ": it is empty");
        }

        var targetEn = newTile.Entity;

        // tried to move onto a tile that has an entity in it
        if (targetEn is not null) {
            var owner = targetEn.Owner;
            if (owner == player) {
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
                targetEn.ProcessDamage(en.MID, attackerDamage);
            }

            if (defenderDamage > 0) {
                en.ProcessDamage(targetEn.MID, defenderDamage);
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