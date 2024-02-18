namespace HexCore.GameMatch.Phases;


/// <summary>
/// Turn end phase
/// </summary>
class TurnEnd : IMatchPhase
{
    public async Task Exec(Match match, Player player)
    {
        match.Emit("turn_end", new(){ {"playerID", player.ID} });
        await match.UpdateOpponents();

        // TODO
        // discard to hand size
        // int discarded = player.PromptDiscard(match.Config.MaxHandSize - player.Hand.Cards.Count, true);
    }
}