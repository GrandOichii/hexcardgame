using System.Net.WebSockets;
using System.Text;

namespace ManagerBack.Services;

/// <summary>
/// Player controller, controlled by a WebSocket connection
/// </summary>
public class WebSocketPlayerController : IPlayerController {
    private readonly WebSocket _socket;

    public WebSocketPlayerController(WebSocket socket)
    {
        _socket = socket;
    }

    private void Write(string message) {
        // var serverMsg = Encoding.UTF8.GetBytes(message);
        // _socket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), WebSocketMessageType.Text, true, CancellationToken.None).Wait();
        _socket.Write(message).Wait();
    }

    private string Read() {
        return _socket.Read().GetAwaiter().GetResult();
        // var buffer = new byte[1024 * 4];
        // _socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).Wait();
        // var result = Encoding.UTF8.GetString(buffer).Replace("\0", string.Empty);

        // return result;
    }

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
