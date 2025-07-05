
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;
using System.ComponentModel.DataAnnotations;
namespace E_Commerce.Models
{

    [CollectionName("Carts")]
    public class Cart
    {
        [BsonId, BsonElement("_id"), BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonRequired, BsonElement("user_id"), BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; }

        [BsonElement("items")]
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        [Required,BsonElement("total"), BsonRepresentation(BsonType.Decimal128)]
        public decimal Total => (decimal)Items.Sum(i => i.Price * i.Quantity);
    }
}