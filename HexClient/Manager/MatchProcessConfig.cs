using System.Text.Json.Serialization;

namespace HexClient.Manager;

public enum BotType {
	RANDOM,
	SMART
}


public static class BotTypeExtensions {
	public static string ToFriendlyString(this BotType status) {
		return status switch
		{
			BotType.RANDOM => "Random",
			BotType.SMART => "Smart",
			_ => "unknown bot type",
		};
	}
}

public class BotConfig {
	public required string StrDeck { get; set; }
	public required string Name { get; set; }
	public required BotType BotType { get; set; }
	public int ActionDelay { get; set; }
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
