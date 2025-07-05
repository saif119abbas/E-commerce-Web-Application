using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDbGenericRepository.Attributes;
using Stripe.Checkout;


namespace E_Commerce.Models
{
    [CollectionName("payment_contexts")] 
    public class PaymentContext
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        [BsonRequired]
        [BsonElement("_id")]
        public Guid Id { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonRequired]
        [BsonElement("payment_intent_id")]
        public Guid PaymentIntentId { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonRequired]
        [BsonElement("user_id")]
        public Guid UserId { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonRequired]
        [BsonElement("stripe_session_id")]
        public string? StripeSessionId { get; set; }

        [BsonRequired]
        [BsonElement("cart")]
        public  Cart? Cart { get; set; }
       /* [BsonRequired]
        [BsonElement("stripe_session")]
        [BsonSerializer(typeof(NewtonsoftJsonSerializer))]
        public  Session ?StripeSession { get; set; }*/
    


        [BsonRequired]
        [BsonElement("reservation_ids")]
        public List<Guid> ReservationIds { get; set; } =new List<Guid>();
        [BsonRequired]
        [BsonElement("products_map")]
        public Dictionary<string, Product> ProductsMap { get; set; } = new Dictionary<string, Product>();

        [BsonRequired]
        [BsonElement("enriched_items")]
        public List<CartItemViewModel>EnrichedItems { get; set; } = new List<CartItemViewModel>();

        [BsonRequired]
        [BsonElement("created_at")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [BsonRequired]
        [BsonElement("expires_at")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(24); 
    }
}
