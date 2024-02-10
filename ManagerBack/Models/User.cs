using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ManagerBack.Models;

public class User {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public bool IsAdmin { get; set; }
}