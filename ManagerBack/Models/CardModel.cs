using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ManagerBack.Models;

/// <summary>
/// DB model, used to represent a card object
/// </summary>
public class CardModel : ExpansionCard {
    /// <summary>
    /// Card DB ID
    /// </summary>
    [JsonIgnore]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
}