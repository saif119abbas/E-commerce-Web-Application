using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDbGenericRepository.Attributes;
using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Models
{
    [CollectionName("Payments")]
    public class Payment
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        [BsonRequired]
        public Guid Id { get; set; } = Guid.NewGuid();

        [BsonElement("customer_id")]
        [BsonRepresentation(BsonType.String)]
        [BsonRequired]
        public Guid CustomerId { get; set; }

        [BsonElement("created_at")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [BsonRequired]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("stripe_session_id")]
        [BsonRequired]
        [StringLength(100)]
        public string StripeSessionId { get; set; }

        [BsonElement("payment_intent_id")]
        [BsonRequired]
        [StringLength(100)]
        public string PaymentIntentId { get; set; }

        [BsonElement("amount")]
        [BsonRepresentation(BsonType.Decimal128)]
        [BsonRequired]
        [Range(0.01, 1000000)]
        public decimal Amount { get; set; }

        [BsonElement("currency")]
        [BsonRequired]
        [StringLength(3)]
        public string Currency { get; set; } = "USD";

        [BsonElement("status")]
        [BsonRequired]
        [StringLength(20)]
        public string Status { get; set; } = "pending";

        [BsonElement("items")]
        public List<PaymentItem> Items { get; set; } = new();

        [BsonElement("metadata")]
        public Dictionary<string, string> Metadata { get; set; } = new();

        [BsonElement("last_updated")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? LastUpdated { get; set; }

        [BsonElement("refunded_amount")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal RefundedAmount { get; set; }

        [BsonElement("description")]
        [StringLength(500)]
        public string? Description { get; set; }
    }


}