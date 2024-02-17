using System.Text.RegularExpressions;

namespace Core.GameMatch.Logging;

/// <summary>
/// Class for logging public messages in match
/// </summary>
public partial class MatchLogger {
    [GeneratedRegex("\\[\\[([^\\[]+)#([^\\[]+)\\]\\]")]
    private static partial Regex CardNameMatcher();

    public List<List<MatchLogEntryPart>> Messages { get; }
    private Match _match;

    public MatchLogger(Match match) {
        _match = match;
        Messages = new();
    }

    public void Log(List<MatchLogEntryPart> message) {
        Messages.Add(message);

        foreach (var player in _match.Players)
            player.NewLogs.Add(message);
    }

    public void ParseAndLog(string message) {
        // TODO leave the task to parse the message to the client
        // TODO add tile location to cards, when moused over will highlight the tile with the card
        
        var groups = CardNameMatcher().Split(message);
        var result = new List<MatchLogEntryPart>();
        // every odd is a match
        // 0 - normal message
        // 1 - card label
        // 2 - actual card name
        var lastMessage = "";
        for (int i = 0; i < groups.Length; i++) {
            var g = groups[i];
            if (g == "") continue;

            var a = i % 3;
            if (a == 0) {
                result.Add(new MatchLogEntryPart(g, ""));
                continue;
            }
            if (a == 1) {
                lastMessage = g;
                continue;
            }

            result.Add(new MatchLogEntryPart(lastMessage, g));
        }
        Log(result);
    }

}