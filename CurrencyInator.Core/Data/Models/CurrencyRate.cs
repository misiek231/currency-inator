using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CurrencyInator.Core.Data.Models;

public class CurrencyRate
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string Currency { get; set; } = null!;
    public DateOnly Date { get; set; }
    public decimal Rate { get; set; }
}
