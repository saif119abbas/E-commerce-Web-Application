using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace E_Commerce.Models
{
    [CollectionName("Orders")]
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonElement("total_cost")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal TotalCost { get; set; }

        [BsonElement("order_date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [BsonElement("customer_id")]
        public Guid CustomerId { get; set; }

        [BsonElement("items")]
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}