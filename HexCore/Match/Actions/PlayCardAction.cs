namespace HexCore.GameMatch.Actions;


/// <summary>
/// Action for playing a card
/// </summary>
class PlayCardAction : IGameAction
{
    private void LogPlayed(Match match, Player player, MatchCard card) {
        match.Logger.ParseAndLog(player.Name + " played " + card.ToLogForm + ".");
    }

    public Task Exec(Match match, Player player, string[] args)
    {
        // args[1] - the MID of the card
        // args[2] - the tile the card is played on
        if (args.Length != 3) {
            if (!match.StrictMode) return Task.CompletedTask;
            throw new Exception("Incorrect number of arguments for play action");
        }

        var mID = args[1];
        var pointRaw = args[2];
        var tile = match.Map.TileAt(pointRaw);
        if (tile is null) {
            if (!match.StrictMode) return Task.CompletedTask;
            throw new Exception("Cannot play a card on point " + pointRaw + ": it is empty");
        }

        var card = player.Hand[mID];
        if (card is null) {
            if (!match.StrictMode) return Task.CompletedTask;
            throw new Exception("Player " + player.ShortStr + " cannot play a card with mID " + mID + ": they don't have it in their hand");
        }

        if (card.IsPlaceable) {
            if (tile.Owner != player) {
                if (!match.StrictMode) return Task.CompletedTask;
                throw new Exception("Can't place entity on tile " + pointRaw + ": it's not owned by " + player.ShortStr);
            }
            if (tile.Entity is object) {
                if (!match.StrictMode) return Task.CompletedTask; 
                throw new Exception("Can't place entity on tile " + pointRaw + ": it is already taken");
            }
            if (!player.TryPlayCard(card)) {
                // failed to play card
                return Task.CompletedTask;
            }

            player.Hand.Cards.Remove(card);
            LogPlayed(match, player, card);
            tile.Entity = card;
            player.AllCards[card] = ZoneTypes.PLACED;

            // call OnEnter function
            card.ExecFunc("OnEnter", card.Data, player.ID, tile.ToLuaTable(match.LState));
            
            // TODO not sure about the ordering, could be weird
            match.Emit("entity_enter", new() { {"mid", card.MID}, {"tile", tile.ToLuaTable(match.LState)} });
            
            return Task.CompletedTask;
        }

        var caster = tile.Entity;
        if (caster is null) {
            if (!match.StrictMode) return Task.CompletedTask;
            throw new Exception("Failed to cast spell " + card.ShortStr + ": tile at " + args[2] + " has no entity");
        }
        if (caster.Owner != player) {
            if (!match.StrictMode) return Task.CompletedTask;
            throw new Exception("Failed to cast spell " + card.ShortStr + ": entity at " + args[2] + " is not owned by the player who played the spell");
        }
        if (!caster.Type.Contains("Mage")) {
            if (!match.StrictMode) return Task.CompletedTask;
            throw new Exception("Failed to cast spell " + card.ShortStr + ": entity at " + args[2] + " is not a Mage");
        }
        // TODO modify spell here

        if (!player.TryPlayCard(card)) {
            // failed to play card
            return Task.CompletedTask;
        }

        player.Hand.Cards.Remove(card); 
        player.AllCards[card] = ZoneTypes.PLAYED;
        // emit the played signal
        match.Emit("spell_cast", new(){ {"casterID", caster.MID}, { "spellID", card.MID } });
        LogPlayed(match, player, card);

        // execute the effect of the card
        card.ExecFunc(MatchCard.EFFECT_FNAME, card.Data, player.ID, caster.Data);
        player.AllCards[card] = ZoneTypes.DISCARD;
        player.Discard.AddToBack(card);

        // TODO demodify spell here

        return Task.CompletedTask;
    }
}
