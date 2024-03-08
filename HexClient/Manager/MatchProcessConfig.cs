using System.Text.Json.Serialization;

namespace HexClient.Manager;

public enum BotType {
	RANDOM,
	SMART
}

public class BotConfig {
	public required string StrDeck { get; set; }
	public required string Name { get; set; }
	public required BotType BotType { get; set; }
}

public class MatchPlayerConfig {
	#nullable enable
	public required BotConfig? BotConfig { get; set; }
	#nullable disable
}

public class MatchProcessConfig {
	[JsonPropertyName("mConfig")]
	public required string MatchConfigId { get; set; }

	public required bool CanWatch { get; set; }

	public required MatchPlayerConfig P1Config { get; set; }
	public required MatchPlayerConfig P2Config { get; set; }
}
