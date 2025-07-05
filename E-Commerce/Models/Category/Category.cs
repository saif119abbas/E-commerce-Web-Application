using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace E_Commerce.Models
{
    [CollectionName("Categories")]
    public class Category
    {
        [BsonId, BsonElement("_id"), BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonElement("name")]
        public string? Name { get; set; }
    }
}
