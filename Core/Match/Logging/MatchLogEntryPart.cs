using System.Text.Json.Serialization;

namespace Core.GameMatch;

/// <summary>
/// Part of the message in the public log
/// </summary>
public struct MatchLogEntryPart {
    [JsonPropertyName("text")]
    public string Text { get; set; }
    [JsonPropertyName("cardRef")]
    public string CardRef { get; set; }

    public MatchLogEntryPart(string message, string cardRef) {
        Text = message;
        CardRef = cardRef;
    }
}