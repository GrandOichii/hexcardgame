using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ManagerBack.Models;

/// <summary>
/// DB model, used to represent a match configuration
/// </summary>
public class MatchConfigModel : MatchConfig {
    /// <summary>
    /// Match configuration DB ID
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /// <summary>
    /// Match configuration name
    /// </summary>
    public required string Name { get; set; }
}