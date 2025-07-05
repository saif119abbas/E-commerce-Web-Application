using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDbGenericRepository.Attributes;

namespace E_Commerce.Models
{
    [CollectionName("product_reservations")]
    public class ProductReservation
    {
        [BsonRequired, BsonId, BsonElement("_id"), BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }


        [BsonRequired, BsonElement("product_id"), BsonRepresentation(BsonType.String)]
        public Guid ProductId { get; set; }

        [BsonRequired, BsonElement("user_id"), BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; }
        [BsonRequired, BsonElement("vendor_name"), BsonRepresentation(BsonType.String)]
        public string ?VendorName { get; set; }

        [BsonRequired, BsonElement("reserved_quantity"), BsonRepresentation(BsonType.Int32)]
        public int ReservedQuantity { get; set; }
        [BsonRequired, BsonElement("reserved_at"), BsonRepresentation(BsonType.DateTime)]
        public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
        [BsonRequired, BsonElement("is_confirmed"), BsonRepresentation(BsonType.Boolean)]
        public bool IsConfirmed { get; set; }
        [BsonRequired, BsonElement("expired_at"), BsonRepresentation(BsonType.DateTime)]
        public DateTime ExpiresAt => ReservedAt.AddMinutes(15);
    }
}
