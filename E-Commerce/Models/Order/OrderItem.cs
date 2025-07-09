using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace E_Commerce.Models
{
    [CollectionName("order_items")]
    public class OrderItem
    {
        [BsonId]
        [BsonRequired]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        [BsonRequired]
        [BsonElement("product_id")]
        public Guid ProductId { get; set; }

        [BsonRequired]
        [BsonElement("order_id")]
        public Guid OrderId { get; set; }

        [BsonElement("vendor_id")]
        public Guid VendorId { get; set; }
        [BsonElement("customer_id")]
        public Guid CustomerId { get; set; }

        [BsonElement("quantity")]
        public int Quantity { get; set; } = 1;

        [BsonElement("price")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; }

        [BsonElement("name")]
        public string ProductName { get; set; }
    }
}