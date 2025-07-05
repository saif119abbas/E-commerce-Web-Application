using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;


namespace E_Commerce.Models
{
    public class CartItem 
      {
           [BsonRequired,BsonElement("product_id"), BsonRepresentation(BsonType.String)]
           public Guid ProductId { get; set; }

           [BsonRequired, BsonElement("vendor_id"), BsonRepresentation(BsonType.String)]
           public Guid VendorId { get; set; }

           [BsonRequired,BsonElement("quantity"), BsonRepresentation(BsonType.Int32)]
           public int Quantity { get; set; } = 1;

           [BsonRequired,BsonElement("price"), BsonRepresentation(BsonType.Decimal128)]
           public decimal? Price { get; set; } 

           [BsonRequired,BsonElement("name"), BsonRepresentation(BsonType.String)]
           public string? ProductName { get; set; }
     }
}
