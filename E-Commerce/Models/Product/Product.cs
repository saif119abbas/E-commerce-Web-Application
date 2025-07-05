using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;
using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Models
{
    [CollectionName("Products")]
    public class Product
    {
        [BsonRequired,BsonId, BsonElement("_id"), BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonRequired,BsonElement("name")]
        public string? Name { get; set; }
        [BsonRequired,BsonElement("price"), BsonRepresentation(BsonType.Decimal128)]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal? Price { get; set; }
        [BsonRequired,BsonElement("quantity"), BsonRepresentation(BsonType.Int32)]
        public int? Quantity { get; set; } = 0;
        [BsonRequired, BsonElement("category_name")]
        public string? CategoryName { get; set; }

        [BsonRequired, BsonElement("vendor_id"), BsonRepresentation(BsonType.String)]
        public Guid VendorId { get; set; }

        [BsonRequired, BsonElement("category_id"), BsonRepresentation(BsonType.String)]
        public Guid CategoryId { get; set; }




    }
}
