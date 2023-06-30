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
        // // replenish source count
        // player.SourceCount = player.MaxSourcePerTurn;

        // // replenish energy
        // player.Energy = player.MaxEnergy;

        // // replenish attacks
        // foreach (var unit in player.Lanes) {
        //     if (unit is null) continue;
        //     unit.ResetAvailableAttacks();
        // }

        // // emit turn start effects
        // match.Emit("turn_start", new(){ {"player", player.ToLuaTable(match.LState)} });

        // // TODO
        // // replenish all units' attacks
        // // foreach (var card in player.InPlay.Cards)
        // //     if (card.Table["availableAttacks"] is not null)
        // //         // TODO replace if units will be able to attack multiple times
        // //         card.Table["availableAttacks"] = 1;

        // // draw for the turn
        // player.DrawCards(match.Config.TurnStartCardDraw);

        // match.UpdateOpponent();
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
        {"aaa", new DoNothingAction() }
    //     { "play", new PlayCardAction() },
    //     { "attack", new AttackAction() }
    };

    public override void Exec(Match match, Player player)
    {
        string action;
        while (true)
        {
            action = PromptAction(match, player);
            var words = action.Split(" ");

            var actionWord = words[0];
            if (actionWord == PASS_TURN_ACTION) break;

            // TODO remove
            if (actionWord == "quit")
            {
                match.Winner = player;
                return;
            }
            
            if (!ACTION_MAP.ContainsKey(actionWord)) throw new Exception("Unknown action from player " + player.Name + ": " + actionWord);

            ACTION_MAP[actionWord].Exec(match, player, words);
            match.View.Update(match);
            // match.UpdateOpponent();
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
        // match.Emit("turn_end", new(){ {"player", player.ToLuaTable(match.LState)} });
        // match.UpdateOpponent();

        // TODO
        // discard to hand size
        // int discarded = player.PromptDiscard(match.Config.MaxHandSize - player.Hand.Cards.Count, true);
    }
}