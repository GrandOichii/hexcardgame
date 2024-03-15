
using System.Collections.Generic;

namespace HexClient.Manager;

public struct Deck {
	public required string Id { get; set; }
	public required string Name { get; set; }
	public required string Description { get; set; }
	public required Dictionary<string, int> Index { get; set; }
}