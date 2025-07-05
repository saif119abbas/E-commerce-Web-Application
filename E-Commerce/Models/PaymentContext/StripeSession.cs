namespace E_Commerce.Models
{
    public class StripeSession
    {
        public string? Id {  get; set; }
        public string? Url { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public string? PaymentIntentId { get; set; }
        public long ?AmountTotal { get; set; }
        public string ?Currency { get; set; }

    }

}
