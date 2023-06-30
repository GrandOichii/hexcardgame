using core.players;

namespace core.match;

abstract class GameAction
{
    abstract public void Exec(Match match, Player player, string[] args);
}

class DoNothingAction : GameAction
{
    public override void Exec(Match match, Player player, string[] args)
    {
    }
}