
namespace HexCore.GameMatch.Players;


/// <summary>
/// Player controller that always just passes it's turn
/// </summary>
public class InactivePlayerController : IPlayerController
{
    public Task CleanUp()
    {
        return Task.CompletedTask;
    }

    public Task SendCard(Match match, Player player, ExpansionCard card)
    {
        return Task.CompletedTask;
    }

    public Task Setup(Player player, Match match)
    {
        return Task.CompletedTask;
    }

    public Task Update(Player player, Match match)
    {
        return Task.CompletedTask;
    }

    public Task<string> DoPickTile(List<int[]> choices, Player player, Match match)
    {
        // TODO
        if (choices.Count == 0) {
        }
        return Task.FromResult("" + choices[0][0] + "." + choices[1][0]);
    }

    public Task<string> DoPromptAction(Player player, Match match)
    {
        return Task.FromResult("pass");
    }
}

