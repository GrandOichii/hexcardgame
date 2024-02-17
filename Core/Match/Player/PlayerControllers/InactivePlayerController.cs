namespace Core.GameMatch.Players;


/// <summary>
/// Player controller that always just passes it's turn
/// </summary>
public class InactivePlayerController : IPlayerController
{
    public void CleanUp()
    {
    }

    public string DoPickTile(List<int[]> choices, Player player, Match match)
    {
        if (choices.Count == 0) {
            // TODO
        }
        return "" + choices[0][0] + "." + choices[1][0];
    }

    public string DoPromptAction(Player player, Match match)
    {
        return "pass";
    }

    public void SendCard(Match match, Player player, ExpansionCard card)
    {
    }

    public void Setup(Player player, Match match)
    {
    }

    public void Update(Player player, Match match)
    {
    }
}

