using System.Text.Json.Serialization;

namespace HexCore.GameMatch;

/// <summary>
/// Part of the message in the public log
/// </summary>
public struct MatchLogEntryPart {
    public string Text { get; set; }
    public string CardRef { get; set; }

    public MatchLogEntryPart(string message, string cardRef) {
        Text = message;
        CardRef = cardRef;
    }
}