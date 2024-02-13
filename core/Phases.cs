using core.players;

namespace core.match;

/// <summary>
/// Match phase
/// </summary>
abstract class MatchPhase {
    abstract public void Exec(Match match, Player player);
}


/// <summary>
/// Turn start phase
/// </summary>
class TurnStart : MatchPhase
{
    public override void Exec(Match match, Player player)
    {
        // increase max energy
        player.MaxEnergy += match.Config.EnergyPerTurn;

        // replenish energy
        player.Energy = player.MaxEnergy;
        

        // // replenish source count
        // player.SourceCount = player.MaxSourcePerTurn;

        // // replenish energy
        // player.Energy = player.MaxEnergy;

        // // replenish attacks
        // foreach (var unit in player.Lanes) {
        //     if (unit is null) continue;
        //     unit.ResetAvailableAttacks();
        // }

        // emit turn start effects
        match.Emit("turn_start", new(){ {"playerID", player.ID} });

        // renew movement and replenish defence

        var map = match.Map;
        for (int i = 0; i < map.Height; i++) {
            for (int j = 0; j < map.Width; j++) {
                var tile = map.Tiles[i, j];

                if (tile is object && tile.Entity is object) {
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
        
        match.View.Update(match);
        match.UpdateOpponents();
    }
}


/// <summary>
/// Main phase of the match
/// </summary>
class MainPhase : MatchPhase
{
    private readonly string PASS_TURN_ACTION = "pass";
    private static readonly Dictionary<string, GameAction> ACTION_MAP =
    new(){
        { "do", new ExecuteCommandAction() },
        { "play", new PlayCardAction() },
        { "move", new MoveAction() },
        // { "get", new GetCardAction() },
    };

    public override void Exec(Match match, Player player)
    {
        string action;
        while (true)
        {
            action = PromptAction(match, player);
            var words = action.Split(" ");

            var actionWord = words[0];
            // if (actionWord == PASS_TURN_ACTION) break;
            // TODO remove
            if (actionWord.CompareTo(PASS_TURN_ACTION) == 0) {
                ++match.PassCount;
                if (match.PassCount >= match.MaxPass) {
                    System.Console.WriteLine("EXCEEDED PASS COUNT");
                    player.Name = "";
                    match.Winner = player;
                }
                break;
            } else {
                match.PassCount = 0;
            }
            
            if (!ACTION_MAP.ContainsKey(actionWord)) {
                if (!match.StrictMode) continue;
                throw new Exception("Unknown action from player " + player.Name + ": " + actionWord);
            }
            ACTION_MAP[actionWord].Exec(match, player, words);
            match.View.Update(match);
            match.UpdateOpponents();
            if (!match.Active) break;                    
        }
    }

    private string PromptAction(Match match, Player player)
    {
        // TODO get all available actions
        return player.Controller.PromptAction(player, match);
    } 
}


/// <summary>
/// Turn end phase
/// </summary>
class TurnEnd : MatchPhase
{
    public override void Exec(Match match, Player player)
    {
        match.Emit("turn_end", new(){ {"playerID", player.ID} });
        match.UpdateOpponents();

        // TODO
        // discard to hand size
        // int discarded = player.PromptDiscard(match.Config.MaxHandSize - player.Hand.Cards.Count, true);
    }
}