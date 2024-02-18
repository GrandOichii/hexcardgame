namespace HexCore.GameMatch.Phases;

/// <summary>
/// Turn start phase
/// </summary>
class TurnStart : IMatchPhase
{
    public async Task Exec(Match match, Player player)
    {
        // increase max energy
        player.MaxEnergy += match.Config.EnergyPerTurn;

        // replenish energy
        player.Energy = player.MaxEnergy;
        
        // emit turn start effects
        match.Emit("turn_start", new(){ {"playerID", player.ID} });

        // renew movement and replenish defence

        var map = match.Map;
        for (int i = 0; i < map.Height; i++) {
            for (int j = 0; j < map.Width; j++) {
                var tile = map.Tiles[i, j];

                if (tile is not null && tile.Entity is not null) {
                    var en = tile.Entity;
                    en.Data["defence"] = en.MaxDefence;
                    if (en.Owner == player && en.IsUnit) {
                        en.Data["movement"] = en.MaxMovement;
                    }
                }
            }
        }

        // draw for the turn
        player.Draw(match.Config.TurnStartDraw);
        
        await match.View.Update(match);
        await match.UpdateOpponents();
    }
}
