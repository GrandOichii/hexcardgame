

using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ManagerBack.Models;

/// <summary>
/// DB model, used to represent a match script
/// </summary>
public class MatchScript {
    /// <summary>
    /// Match script DB ID
    /// </summary>
    [JsonIgnore]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /// <summary>
    /// Match script name
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Match script text
    /// </summary>
    public required string Script { get; set; }
}