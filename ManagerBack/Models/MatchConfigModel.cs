using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ManagerBack.Models;

public class MatchConfigModel : MatchConfig {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
}