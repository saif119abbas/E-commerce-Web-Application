using E_Commerce.Models;
using Stripe;
using Stripe.Checkout;
namespace E_Commerce.Services
{
    public interface IStripePaymentService
    {
        public Task<Session> CreateCheckoutSessionAsync(Guid userId,List<CartItemViewModel> items,string clientReferenceId = null);
        public Refund RefundPayement(string stripeSessionId, List<CartItemViewModel> items);
    }
}
