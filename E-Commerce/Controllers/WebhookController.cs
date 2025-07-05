using E_Commerce.Configuration;
using E_Commerce.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using CustomerService = Stripe.CustomerService;

namespace E_Commerce.Controllers
{
    [ApiController]
    [Route("api/stripe-webhooks")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<StripeWebhookController> _logger;
        private readonly StripeSettings _stripeSettings;

        public StripeWebhookController(
            ICustomerService customerService,
            ILogger<StripeWebhookController> logger,
            IOptions<StripeSettings> stripeSettings)
        {
            _customerService = customerService;
            _logger = logger;
            _stripeSettings = stripeSettings.Value;
        }

        [HttpPost]
        public async Task<IActionResult> Index()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
              
                var stripeEvent = EventUtility.ParseEvent(json);
                var signatureHeader = Request.Headers["Stripe-Signature"];
                var webhookSecret = _stripeSettings.WebhookSecret;

                stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, webhookSecret);

                _logger.LogInformation("Received Stripe event: {Type}", stripeEvent.Type);

                switch (stripeEvent.Type)
                {
                    case Events.CustomerCreated:
                        var customerObj = stripeEvent.Data.Object as Customer;
                        if (customerObj != null)
                        {
                            var customerService = new CustomerService();
                            var latestCustomer = await customerService.GetAsync(customerObj.Id);
                           
                            _logger.LogInformation("Customer created: {Id}", latestCustomer.Id);
                        }
                        break;

                    case Events.CheckoutSessionAsyncPaymentSucceeded:
                        var session = stripeEvent.Data.Object as Session ;

                        if (session != null)
                        {
                            await _customerService.HandlePaymentSuccessAsync(session);
                        }
                            _logger.LogInformation("PaymentIntent succeeded: {Id}", session!.PaymentIntentId);
                        break;

                    case Events.CheckoutSessionCompleted:
                        var checkoutSessionCompleted = stripeEvent.Data.Object as Session;
                        //"cs_test_a1uEMEIY6SAnHcBocWy8K6va086AKNZ4wPXB1elkaA5nJ62nEwirdQ0wc8"

                        if (checkoutSessionCompleted != null)
                        {
                            await _customerService.HandlePaymentSuccessAsync(checkoutSessionCompleted);
                        }
                        _logger.LogInformation("Session Id {Id}", checkoutSessionCompleted!.Id);
                        break;

                    case Events.CheckoutSessionAsyncPaymentFailed:
                        var FailuerSession = stripeEvent.Data.Object as Session;
                    
                        if (FailuerSession != null )
                        {
                            await _customerService.HandlePaymentFailureAsync(FailuerSession);
                            _logger.LogInformation("PaymentIntent failed: {Id}", FailuerSession.Id);
                        }
                        break;

                    /*case EventTypes.CheckoutSessionCompleted:
                        var session = stripeEvent.Data.Object as Session;
                        _logger.LogInformation("Checkout session completed: {Id}", session?.Id);
                        break;*/

                    default:
                        _logger.LogWarning("Unhandled Stripe event type: {Type}", stripeEvent.Type);
                        break;
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error while processing webhook");
                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while processing webhook");
                return BadRequest();
            }
        }
    }
}
