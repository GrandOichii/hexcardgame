namespace Core.GameMatch.Phases;


/// <summary>
/// Turn end phase
/// </summary>
class TurnEnd : IMatchPhase
{
    public void Exec(Match match, Player player)
    {
        match.Emit("turn_end", new(){ {"playerID", player.ID} });
        match.UpdateOpponents();

        // TODO
        // discard to hand size
        // int discarded = player.PromptDiscard(match.Config.MaxHandSize - player.Hand.Cards.Count, true);
    }
}