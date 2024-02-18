namespace HexCore.GameMatch.Players;


/// <summary>
/// Player controller, controlled by a TCP socket
/// </summary>
public abstract class IOPlayerController : IPlayerController
{
    /// <summary>
    /// Writes the message to the socket
    /// </summary>
    /// <param name="message">Message</param>
    public abstract Task Write(string message);

    /// <summary>
    /// Reads a message from socket
    /// </summary>
    /// <returns>The read message</returns>
    public abstract Task<string> Read();

    public abstract Task CleanUp();

    public async Task<string> DoPromptAction(Player player, Match match)
    {
        var state = new MatchState(match, player, "action");

        await Write(state.ToJson());
        
        return await Read();
    }

    public async Task Setup(Player player, Match match)
    {
        await Write(new MatchInfoState(player, match).ToJson());
    }

    public async Task Update(Player player, Match match)
    {
        await Write(new MatchState(match, player, "update").ToJson());
    }

    public async Task<string> DoPickTile(List<int[]> choices, Player player, Match match)
    {
        var request = "pt";
        var args = new List<string>();
        for (int i = 0; i < choices.Count; i++) {
            args.Add("" + choices[i][0] + "." + choices[i][1]);
        }
        await Write(new MatchState(match, player, request, args).ToJson());
        
        return await Read();
    }

    public async Task SendCard(Match match, Player player, ExpansionCard card)
    {
        await Write(card.ToJson());
    }

}
