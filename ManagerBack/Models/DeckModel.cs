using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ManagerBack.Models;

public class DeckModel {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id;

    [BsonRepresentation(BsonType.ObjectId)]
    public required string OwnerId { get; set; }

    public required string Name { get; set; }
    public required string Description { get; set; }
    public Dictionary<string, int> Index { get; set; } = new();
}