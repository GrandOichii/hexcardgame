using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ManagerBack.Models;

/// <summary>
/// DB model, used to represent a user object 
/// </summary>
public class User {
    /// <summary>
    /// User DB ID
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /// <summary>
    /// Username
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// Hashed password
    /// </summary>
    public required string PasswordHash { get; set; }

    /// <summary>
    /// Admin flag
    /// </summary>
    public bool IsAdmin { get; set; }
}