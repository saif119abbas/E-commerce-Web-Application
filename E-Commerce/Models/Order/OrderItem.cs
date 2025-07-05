using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace E_Commerce.Models
{
    public class OrderItem
    {
        [BsonElement("product_id")]
        public Guid ProductId { get; set; }

        [BsonElement("vendor_id")]
        public Guid VendorId { get; set; }

        [BsonElement("quantity")]
        public int Quantity { get; set; } = 1;

        [BsonElement("price")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; }

        [BsonElement("name")]
        public string ProductName { get; set; }
    }
}