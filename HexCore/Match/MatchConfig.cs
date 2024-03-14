using System.Text.Json;
using System.Text.Json.Serialization;

namespace HexCore.GameMatch;

/// <summary>
/// Configuraton object for match creation
/// </summary>
public class MatchConfig
{
    public int StartingEnergy { get; set; } = 0;
    public int EnergyPerTurn { get; set; } = 1;
    public int MaxEnergy { get; set; } = -1;
    public int StartingHandSize { get; set; }
    public int TurnStartDraw { get; set; }
    public int? Seed { get; set; }
    public required string SetupScript { get; set; }
    public List<List<int>> Map { get; set; } = new();
    public List<string> AddonPaths { get; set; } = new();

    /// <summary>
    /// Creates the match configuration from JSON
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    static public MatchConfig FromJson(string text) {
        var result = JsonSerializer.Deserialize<MatchConfig>(text, Common.JSON_SERIALIZATION_OPTIONS) ?? throw new Exception("Failed to parse MatchConfig from " + text);
        return result;
    }
}
