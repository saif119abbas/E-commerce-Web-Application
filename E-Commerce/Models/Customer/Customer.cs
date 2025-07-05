using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace E_Commerce.Models

{
    [CollectionName("Customers")]
    public class Customer : User
    {
        [BsonRequired, BsonElement("user_id"), BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; } 
    }
}
