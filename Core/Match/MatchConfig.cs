using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.GameMatch;



/// <summary>
/// Configuraton object for match creation
/// </summary>
public class MatchConfig
{
    static readonly Random _rnd = new();

    [JsonPropertyName("startingEnergy")]
    public int StartingEnergy { get; set; } = 0;
    [JsonPropertyName("energyPerTurn")]
    public int EnergyPerTurn { get; set; } = 1;
    [JsonPropertyName("maxEnergy")]
    public int MaxEnergy { get; set; } = -1;
    [JsonPropertyName("startingHandSize")]
    public int StartingHandSize { get; set; }
    [JsonPropertyName("turnStartDraw")]
    public int TurnStartDraw { get; set; }
    [JsonPropertyName("seed")]
    public int Seed { get; set; } = -1;
    [JsonPropertyName("setupScript")]
    public string SetupScript { get; set; }="error('NO SETUP SCRIPT')";
    [JsonPropertyName("map")]
    public List<List<int>> Map { get; set; }=new();

    [JsonPropertyName("addons")]
    public List<string> AddonPaths { get; set; } = new();

    /// <summary>
    /// Creates the match configuration from JSON
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    static public MatchConfig FromJson(string text) {
        var result = JsonSerializer.Deserialize<MatchConfig>(text) ?? throw new Exception("Failed to parse MatchConfig from " + text);
        if (result.Seed == -1) {
            result.Seed = _rnd.Next();
        }
        return result;
    }
}
