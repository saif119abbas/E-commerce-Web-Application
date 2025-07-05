using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace E_Commerce.Models
{
    [CollectionName("Vendors")]
    public class Vendor:User
    {
        [BsonRequired, BsonElement("user_id"), BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; }
      /*  [BsonElement("products")]
        public List<Product> Products { get; set; } = new List<Product>();*/
    }
}
