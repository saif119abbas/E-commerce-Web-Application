using E_Commerce.Models;
using Microsoft.Extensions.Options;
using Stripe.Checkout;
using Stripe;
using E_Commerce.Configuration;

namespace E_Commerce.Services
{

    public class StripePaymentService : IStripePaymentService
    {
 
        private readonly StripeSettings _stripeSettings;
        public StripePaymentService(IOptions<StripeSettings> stripeSettings)
        {
            StripeConfiguration.ApiKey = stripeSettings.Value.SecretKey;
            _stripeSettings = stripeSettings.Value;
        }

        public Refund RefundPayement(string stripeSessionId, List<CartItemViewModel> items)
        {
            var amount = (long)items.Sum(i => (long)i.Total*100);
            var options = new RefundCreateOptions
            {
              Reason="failed to paid",
              PaymentIntent= stripeSessionId,
              Amount= amount

            };
            var service = new RefundService();
            Refund refund = service.Create(options);
            return refund;
        }

        public  async Task<Session> CreateCheckoutSessionAsync(Guid userId,List<CartItemViewModel> items,string clientReferenceId = null)
        {
            var lineItems = items.Select(item => new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(item.Price * 100), 
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.ProductName,
                        Metadata = new Dictionary<string, string>
                    {
                        { "ProductId", item.ProductId.ToString()},
                        { "UserId", userId.ToString() }
                    }
                    },
                },
                Quantity = item.Quantity,
            }).ToList();
            var successUrl = _stripeSettings.SuccessUrl;
            var cancelUrl= _stripeSettings.CancelUrl;
            var options = new SessionCreateOptions
            {
                //PaymentMethodTypes = new List<string>{"card"},
                Mode = "payment",
                
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                CustomerEmail = "saifaldawlaabbas@gmail.com",
                LineItems = lineItems,
                Metadata = new Dictionary<string, string>
                {
                    { "UserId", userId.ToString() },
                    { "CartItemsCount", items.Count.ToString() }
                },
                PaymentIntentData = new SessionPaymentIntentDataOptions
                {
                    Metadata = new Dictionary<string, string>
                {
                    { "UserId", userId.ToString() }
                }
                }
            };
            var service = new SessionService();
            Session session = await service.CreateAsync(options);
            session.PaymentIntentId=Guid.NewGuid().ToString();



            return session;
        }
    }
}