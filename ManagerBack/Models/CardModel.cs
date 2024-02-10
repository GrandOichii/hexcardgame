using core.cards;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ManagerBack.Models;


public class CardModel : ExpansionCard {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id;

    public string GetCID() => CID;

}