namespace Core.GameMatch.Phases;


/// <summary>
/// Main phase of the match
/// </summary>
class MainPhase : IMatchPhase
{
    private readonly string PASS_TURN_ACTION = "pass";
    private static readonly Dictionary<string, IGameAction> ACTION_MAP =
    new(){
        { "do", new ExecuteCommandAction() },
        { "play", new PlayCardAction() },
        { "move", new MoveAction() },
        // { "get", new GetCardAction() },
    };

    public void Exec(Match match, Player player)
    {
        string action;
        while (true)
        {
            action = PromptAction(match, player);
            var words = action.Split(" ");

            var actionWord = words[0];
            // if (actionWord == PASS_TURN_ACTION) break;
            if (actionWord == PASS_TURN_ACTION) {
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
