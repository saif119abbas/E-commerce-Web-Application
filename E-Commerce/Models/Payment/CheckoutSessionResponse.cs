namespace E_Commerce.Models
{
    public class CheckoutSessionResponse
    {
        public string? SessionId { get; set; }
        public string ?Url { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
