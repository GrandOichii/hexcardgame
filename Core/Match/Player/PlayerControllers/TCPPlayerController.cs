using System.Net.Sockets;
using System.Text;
using Shared;

namespace Core.GameMatch.Players;


/// <summary>
/// Player controller, controlled by a TCP socket
/// </summary>
public class TCPPlayerController : IPlayerController
{
    private readonly TcpClient _handler;

    public TCPPlayerController(TcpListener listener, Match match) {
        match.SystemLogger.Log("TCPPlayerController", "Waiting for connection");

        _handler = listener.AcceptTcpClient();
        
        match.SystemLogger.Log("TCPPlayerController", "Connection established, sending match info");
    }

    /// <summary>
    /// Writes the message to the socket
    /// </summary>
    /// <param name="message">Message</param>
    public void Write(string message) {
        var data = Encoding.UTF8.GetBytes(message);
        var handler = _handler.GetStream();
        NetUtil.Write(handler, message);
    }

    /// <summary>
    /// Reads a message from socket
    /// </summary>
    /// <returns>The read message</returns>
    public string Read() {
        // return Console.ReadLine();
        
        var stream = _handler.GetStream();
        var result = NetUtil.Read(stream);
        return result;
    }

    // public void InformMatchEnd(Player controlledPlayer, Match match, bool won) {
    //     Write(MatchParsers.CreateMState(controlledPlayer, match, (won ? "won" : "lost"), new()).ToJson());
    // }

    // public string ProcessPickAttackTarget(Player controlledPlayer, Match match, CardW card) {
    //     // TODO replace with available attacks
    //     var opponent = match.OpponentOf(controlledPlayer);
    //     var targets = new List<string>();
    //     foreach (var treasure in opponent.Treasures.Cards)
    //         targets.Add(treasure.GetCardWrapper().ID);

    //     Write(MatchParsers.CreateMState(controlledPlayer, match, "pick attack target", targets, "", card.ID).ToJson());
    //     return Read();
    // }

    public string DoPromptAction(Player player, Match match)
    {
        var state = new MatchState(match, player, "action");

        Write(state.ToJson());
        
        return Read();
    }

    public void Setup(Player player, Match match)
    {
        Write(new MatchInfoState(player, match).ToJson());
    }

    public void Update(Player player, Match match)
    {
        Write(new MatchState(match, player, "update").ToJson());
    }

    public void CleanUp()
    {
        // TODO
        _handler.Close();
    }

    public string DoPickTile(List<int[]> choices, Player player, Match match)
    {
        var request = "pt";
        var args = new List<string>();
        for (int i = 0; i < choices.Count; i++) {
            args.Add("" + choices[i][0] + "." + choices[i][1]);
        }
        Write(new MatchState(match, player, request, args).ToJson());
        
        return Read();
    }

    public void SendCard(Match match, Player player, ExpansionCard card)
    {
        // var state = new MatchState(match, player, "card", new(){card.ToJson()});

        // Write(state.ToJson());
        Write(card.ToJson());
    }
}
