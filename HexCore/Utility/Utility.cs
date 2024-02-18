using System.Text.Json;

namespace Utility;

/// <summary>
/// General utility class
/// </summary>
static class Common {
    /// <summary>
    /// Returns the shuffled list
    /// </summary>
    /// <param name="list">List</param>
    /// <param name="rnd">Random number generator</param>
    /// <typeparam name="T">Type of contained value</typeparam>
    /// <returns>The shuffled list</returns>
    static public List<T> Shuffled<T>(List<T> list, Random rnd) {
        return list.OrderBy(a => rnd.Next()).ToList();
    }

    static public readonly JsonSerializerOptions JSON_SERIALIZATION_OPTIONS = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}