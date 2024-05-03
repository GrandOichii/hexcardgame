using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ManagerBack.Models;

/// <summary>
/// DB model, used to represent a deck model
/// </summary>
public class DeckModel {
    /// <summary>
    /// Deck DB ID
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /// <summary>
    /// User ID of the owner
    /// </summary>
    [BsonRepresentation(BsonType.ObjectId)]
    public required string OwnerId { get; set; }

    /// <summary>
    /// Deck name
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Deck description
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Card ID to amount mapping
    /// </summary>
    public Dictionary<string, int> Index { get; set; } = new();
}