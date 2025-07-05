using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Models
{
    public class PaymentItem
    {
        [BsonElement("product_id")]
        [BsonRepresentation(BsonType.String)]
        public Guid ProductId { get; set; }

        [BsonElement("quantity")]
        [Range(1, 1000)]
        public int Quantity { get; set; }

        [BsonElement("price")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }
    }
}
