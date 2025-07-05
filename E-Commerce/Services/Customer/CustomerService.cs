using com.sun.security.ntlm;
using E_Commerce.Models;
using E_Commerce.Repositories;
using E_Commerce.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;
using Stripe;
using Stripe.Checkout;
using Order = E_Commerce.Models.Order;
using Product = E_Commerce.Models.Product;


namespace E_Commerce.Services
{
    public class CustomerService:ICustomerService
    {
        private readonly IProductRepository _productRepository;
        private readonly IVendorRepository _vendorRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductReservationRepository _productReservationRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IPaymentContextRepository _paymentContextRepository;
        private readonly IElasticsearchService<ProductItem> _elasticsearchService;
        private readonly IStripePaymentService _stripePaymentService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<VendorService> _logger;
        private readonly IMemoryCache _memoryCache;
        public CustomerService(IProductRepository productRepository, 
                                IVendorRepository vendorRepository,
                                ICategoryRepository categoryRepository,
                                ICartRepository cartRepository,
                                IOrderRepository orderRepository,
                                IProductReservationRepository productReservationRepository,
                                IPaymentRepository paymentRepository,
                                IPaymentContextRepository paymentContextRepository,
                                IElasticsearchService<ProductItem> elasticsearchService,
                                IStripePaymentService stripePaymentService,
                                IUnitOfWork unitOfWork,
                                ILogger<VendorService> logger,
                                IMemoryCache memoryCache
            ) 
        { 
            _productRepository = productRepository;
            _vendorRepository = vendorRepository;
            _categoryRepository = categoryRepository;
            _cartRepository = cartRepository;
            _orderRepository = orderRepository;
            _productReservationRepository = productReservationRepository;
            _paymentRepository = paymentRepository;
            _paymentContextRepository = paymentContextRepository;
            _elasticsearchService = elasticsearchService;
            _stripePaymentService = stripePaymentService;
            _cartRepository = cartRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public async Task<OperationResult<CartViewModel>> UpdateCartAsync(string userId, string productId, int quantity = 1)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) 
                    || string.IsNullOrEmpty(productId) 
                    || !Guid.TryParse(userId, out Guid parsedUserId)
                    || !Guid.TryParse(productId, out Guid parsedProductId))
                {
                    return OperationResult<CartViewModel>.FailureResult(400, "Invalid parameters");
                }
                
                await _unitOfWork.StartTransactionAsync();
                var session=_unitOfWork.Session;

                var getCartResult = await _cartRepository.GetCartAsync(parsedUserId);

                if (getCartResult == null || getCartResult.Data == null || !getCartResult.Success)
                {
                    await _unitOfWork.AbortAsync();
                    var error = getCartResult?.Errors != null
                        ? string.Join(", ", getCartResult.Errors)
                        : "Something went wrong";
                    return OperationResult<CartViewModel>.FailureResult(getCartResult?.StatusCode ?? 500, error);
                }
                var cart=getCartResult?.Data;
                var getProductResult = await _productRepository.GetProductAsync(parsedProductId);
                if (getProductResult == null || !getProductResult.Success || getProductResult.Data == null)
                {
                    await _unitOfWork.AbortAsync();
                    var error = getProductResult?.Errors != null
                        ? string.Join(", ", getProductResult.Errors)
                        : "Something went wrong";
                    return OperationResult<CartViewModel>.FailureResult(getProductResult?.StatusCode ?? 500, error);
                }
                var product= getProductResult.Data;
                var existingItem = cart!.Items.FirstOrDefault(i => i.ProductId == parsedProductId);
               
                if (existingItem != null)
                {
                    if(quantity == 0)
                    {
                        cart.Items.Remove(existingItem);
                    }
                    else
                    {
                        var newQuantity = existingItem.Quantity + quantity;
                        if (product.Quantity < quantity)
                        {
                            await _unitOfWork.AbortAsync();
                            return OperationResult<CartViewModel>.FailureResult(400, "Not enough quantity");

                        }
                        existingItem.Quantity = newQuantity;
                    }
                
                    
                }
                else
                {
                    if (quantity == 0)
                    {
                        await _unitOfWork.AbortAsync();
                        return OperationResult<CartViewModel>.FailureResult(404, 
                            "This is not found or may be that has been removed");
                    }
                    if (product.Quantity==0)
                    {
                        await _unitOfWork.AbortAsync();
                        return OperationResult<CartViewModel>.FailureResult(400, "Not enough quantity");

                    }
                    cart.Items.Add(new CartItem
                    {
                        ProductId = product.Id,
                        VendorId = product.VendorId,
                        ProductName = product.Name,
                        Price = product.Price,
                        Quantity = quantity,
                   
                    });
                }
                var addToCartResult=await _cartRepository.UpdateCartAsync(cart, session);
                if (addToCartResult == null || !addToCartResult.Success || addToCartResult.Data == null)
                {
                    await _unitOfWork.AbortAsync();
                    var error = addToCartResult?.Errors != null
                        ? string.Join(", ", addToCartResult.Errors)
                        : "Something went wrong";
                    return OperationResult<CartViewModel>.FailureResult(addToCartResult?.StatusCode ?? 500, error);
                }
                await _unitOfWork.CommitAsync();
                var result = new CartViewModel
                {
                    Id=cart.Id,
                    Items=cart.Items.Select(i=>new CartItemViewModel
                    {
                        ProductId=i.ProductId,
                        ProductName=product.Name,
                        Price= (decimal)product.Price!,
                        Quantity=i.Quantity,
                    }
                    ).ToList(),
                };
                return OperationResult<CartViewModel>.SuccessResult(result);


            }
            catch (Exception ex)
            {
               await _unitOfWork.AbortAsync();
                return OperationResult<CartViewModel>.FailureResult(500,ex.Message);
            }
        }

        public async Task<OperationResult<List<ProductItem>>> GetAllProductsAllAsync()
        {
            try
            {
                var getProductsResult = await _elasticsearchService.GetAllAsync();

                if (getProductsResult == null || !getProductsResult.Success || getProductsResult.Data == null)
                {
                    var error = getProductsResult?.Errors != null
                        ? string.Join(", ", getProductsResult.Errors)
                        : "Something went wrong";
                    return OperationResult<List<ProductItem>>.FailureResult(getProductsResult?.StatusCode ?? 500, error);
                }

                var productItems = getProductsResult.Data;
                return OperationResult<List<ProductItem>>.SuccessResult(productItems);
            }
            catch (Exception ex)
            {
                return OperationResult<List<ProductItem>>.FailureResult(500, ex.Message);
            }
        }

        public async Task<OperationResult<CartViewModel>> GetCartAsync(string userId)
        {
            try
            {
                if (!Guid.TryParse(userId, out var parsedUserId))
                {
                    return OperationResult<CartViewModel>.FailureResult(400, "Invalid parmeters");
                }
                var getCartResult = await _cartRepository.GetCartAsync(parsedUserId);
                if (getCartResult.Data == null || getCartResult == null || !getCartResult.Success)
                {
                    var error = getCartResult?.Errors != null
                          ? string.Join(", ", getCartResult.Errors)
                          : "Something went wrong ";

                    return OperationResult<CartViewModel>.FailureResult(getCartResult?.StatusCode ?? 500, error);
                }
                var cart = new CartViewModel
                {
                    Id = getCartResult.Data.Id,
                    Items = getCartResult.Data?.Items != null
                      ? getCartResult.Data.Items.Select(item => new CartItemViewModel
                      {
                          ProductId = item.ProductId,
                          Price = (decimal)item.Price!,
                          Quantity = item.Quantity,
                          ProductName = item.ProductName
                      }).ToList()
                      : new List<CartItemViewModel>()
                };

                return OperationResult<CartViewModel>.SuccessResult(cart);
            }
            catch (Exception ex)
            {
                return OperationResult<CartViewModel>.FailureResult(500, ex.Message);
            }

        }
        public async Task<OperationResult<CheckoutSessionResponse>> CheckoutAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var parsedUserId))
                return OperationResult<CheckoutSessionResponse>.FailureResult(400, "Invalid user ID");

            try
            {
                await _unitOfWork.StartTransactionAsync();
                var session = _unitOfWork.Session;
               // var customerResult=await

                var validationResult = await ValidateCartAndProducts(parsedUserId, session);
                if (validationResult == null || !validationResult.Success)
                {
                    var error = validationResult!.Errors != null
                    ? string.Join(", ", validationResult.Errors)
                    : "Valdation error for products";
                    return OperationResult<CheckoutSessionResponse>.FailureResult(validationResult?.StatusCode ?? 500, error);
                }

                var (cart, productMap, enrichedItems) = validationResult.Data;
                var reservationResult = await CreateTemporaryReservations(cart, parsedUserId, productMap, session);
                if (reservationResult == null || reservationResult.Data == null || !reservationResult.Success)
                {
                    var error = reservationResult!.Errors != null
                    ? string.Join(", ", reservationResult.Errors)
                    : "Valdation error for products";
                    return OperationResult<CheckoutSessionResponse>
                        .FailureResult(reservationResult?.StatusCode ?? 500, error);
                    
                }

                var newReservations = reservationResult.Data;
               

                var stripeSession = await _stripePaymentService.CreateCheckoutSessionAsync(
                    parsedUserId,
                    enrichedItems,
                    $"cart_{parsedUserId}_{DateTime.UtcNow.Ticks}");

                if (stripeSession == null)
                {
                    await _unitOfWork.AbortAsync();
                    return OperationResult<CheckoutSessionResponse>.FailureResult(500, "Failed to create Stripe session");
                }
                PaymentContext context = new PaymentContext
                {
                    Id=Guid.NewGuid(),
                    PaymentIntentId=Guid.Parse(stripeSession.PaymentIntentId),
                    StripeSessionId= stripeSession.Id,
                    UserId=parsedUserId,
                    Cart=cart,
                    ReservationIds= newReservations.Select(r => r.Id).ToList(),
                    ProductsMap = productMap,
                    EnrichedItems=enrichedItems
              
                };
                var contextResult = await _paymentContextRepository.CreateAsync(context, session);
                if (contextResult == null || contextResult.Data == null || !contextResult.Success)
                {
                    var error = contextResult!.Errors != null
                    ? string.Join(", ", contextResult.Errors)
                    : "Valdation error for products";
                    return OperationResult<CheckoutSessionResponse>
                        .FailureResult(contextResult?.StatusCode ?? 500, error);

                }
              

                await _unitOfWork.CommitAsync();
          
                return OperationResult<CheckoutSessionResponse>.SuccessResult(
                    new CheckoutSessionResponse
                    {
                        SessionId = stripeSession.Id,
                        Url = stripeSession.Url,
                        ExpiresAt = stripeSession.ExpiresAt
                    });
            }
            catch (StripeException ex)
            {
                await _unitOfWork.AbortAsync();
                _logger.LogError(ex, "Stripe error during checkout for user {UserId}", userId);
                return OperationResult<CheckoutSessionResponse>.FailureResult(502, ex.Message);
            }
            catch (Exception ex)
            {
                await _unitOfWork.AbortAsync();
                _logger.LogError(ex, "Checkout failed for user {UserId}", userId);
                return OperationResult<CheckoutSessionResponse>.FailureResult(
                    500, "Checkout process failed. Please try again.");
            }
        }
        public async Task<OperationResult<bool>> HandlePaymentSuccessAsync(Session stripeSession)
        {
            await _unitOfWork.StartTransactionAsync();
            var session = _unitOfWork.Session;

            try
            {
             
                var contextResult = await _paymentContextRepository.GetAsync(stripeSession.Id);
                if (contextResult == null || contextResult.Data == null || !contextResult.Success)
                {
                    var error = contextResult!.Errors != null
                    ? string.Join(", ", contextResult.Errors)
                    : "Valdation error for products";
                    return OperationResult<bool>
                        .FailureResult(contextResult?.StatusCode ?? 500, error);

                }

           
                var reservationIds = contextResult.Data.ReservationIds;
                var productMap = contextResult.Data.ProductsMap;
                var enrichedItems = contextResult.Data.EnrichedItems;
                var cart = contextResult.Data.Cart;
              

                // 3. Process fulfillment
                var reservationsResult = await _productReservationRepository.GetProductsAsync(reservationIds);
                if (reservationsResult == null || reservationsResult.Data == null || !reservationsResult.Success)
                {
                    var error = reservationsResult!.Errors != null
                    ? string.Join(", ", reservationsResult.Errors)
                    : "Valdation error for products";
                    return OperationResult<bool>
                        .FailureResult(reservationsResult?.StatusCode ?? 500, error);

                }
                

                var fulfillmentResult = await FulfillOrder(
                    stripeSession,
                    contextResult.Data.UserId,
                    cart,
                    reservationsResult.Data,
                    productMap,
                    enrichedItems,
                    session);

                var paymentDeleteResult=await _paymentContextRepository.DeleteAsync(contextResult.Data.Id, session);
                if (paymentDeleteResult == null  || !paymentDeleteResult.Success)
                {
                    var error = paymentDeleteResult!.Errors != null
                    ? string.Join(", ", paymentDeleteResult.Errors)
                    : "Unable to delete payment";
                    return OperationResult<bool>
                        .FailureResult(paymentDeleteResult?.StatusCode ?? 500, error);

                }

                await _unitOfWork.CommitAsync();
                return OperationResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                await _unitOfWork.AbortAsync();
                _logger.LogError(ex, "Payment success handling failed");
                return OperationResult<bool>.FailureResult(500, "Payment processing failed");
            }
        }

        public async Task<OperationResult<bool>> HandlePaymentFailureAsync(Session stripeSession)
        {
            await _unitOfWork.StartTransactionAsync();
            var session = _unitOfWork.Session;
            try
            {

                var contextResult = await _paymentContextRepository.GetAsync(stripeSession.Id);
                if (contextResult == null || contextResult.Data == null || !contextResult.Success)
                {
                    var error = contextResult!.Errors != null
                    ? string.Join(", ", contextResult.Errors)
                    : "Valdation error for products";
                    return OperationResult<bool>
                        .FailureResult(contextResult?.StatusCode ?? 500, error);

                }
               
                var reservationIds = contextResult.Data.ReservationIds;
                var cancelOrderResult = await CancelOrder(reservationIds, session);

                return OperationResult<bool>.SuccessResult(true);
            }
            catch
            {
                await _unitOfWork.AbortAsync();
                return OperationResult<bool>.FailureResult(500, "Payment processing failed");
            }
       
        }
        private async Task<OperationResult<bool>> CancelOrder(
        List<Guid> productReservations, IClientSessionHandle clientSession
            )
        {
            try
            {
                await _unitOfWork.StartTransactionAsync();
                var deleteReservation = await _productReservationRepository.DeleteAsync(productReservations, clientSession);
                if (deleteReservation == null  || !deleteReservation.Success)
                {
                    var error = deleteReservation!.Errors != null
                    ? string.Join(", ", deleteReservation.Errors)
                    : "Valdation error for products";
                    return OperationResult<bool>
                        .FailureResult(deleteReservation?.StatusCode ?? 500, error);

                }
            
                return OperationResult<bool>.SuccessResult(true);


            }
            catch(Exception ex)
            {
                await _unitOfWork.AbortAsync();
                _logger.LogError(ex, "Payment failure handling failed");
                return OperationResult<bool>.FailureResult(500, "Payment failure processing failed");
            }
        }

        private async Task<OperationResult<(Cart, Dictionary<string, Product>, List<CartItemViewModel>)>>
            ValidateCartAndProducts(Guid userId, IClientSessionHandle session)
        {
            var cartResult = await _cartRepository.GetCartAsync(userId);
            if (cartResult==null || cartResult.Data == null || !cartResult.Success)
            {
                
                var error = cartResult!.Errors != null
                    ? string.Join(", ", cartResult.Errors)
                    : "Failed to retrieve cart";
                return OperationResult<(Cart, Dictionary<string, Product>, List<CartItemViewModel>)>
                    .FailureResult(cartResult?.StatusCode ?? 500, error);
            }

            var cart = cartResult.Data;
            var productIds = cart.Items.Select(i => i.ProductId).ToList();
            var reservationTask = _productReservationRepository.GetProductsAsync(productIds);
            var productTask = _productRepository.GetProductsAsync(productIds);

            await Task.WhenAll(reservationTask, productTask);
            var reservationsResult = await reservationTask;
            var productsResult = await productTask;
            

            if (!reservationsResult.Success || reservationsResult.Data == null)
            {
                var error = reservationsResult.Errors != null
                    ? string.Join(", ", reservationsResult.Errors)
                    : "Failed to retrieve reservations";
                return OperationResult<(Cart, Dictionary<string, Product>, List<CartItemViewModel>)>
                    .FailureResult(reservationsResult?.StatusCode ?? 500, error);
            }

            if (!productsResult.Success || productsResult.Data == null)
            {
                var error = productsResult.Errors != null
                    ? string.Join(", ", productsResult.Errors)
                    : "Failed to retrieve products";
                return OperationResult<(Cart, Dictionary<string, Product>, List<CartItemViewModel>)>
                    .FailureResult(productsResult?.StatusCode ?? 500, error);
            }

            var productMap = productsResult.Data.ToDictionary(p => p.Id.ToString());
            var reservationMap = reservationsResult.Data
                .GroupBy(r => r.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.ReservedQuantity));

            var (isValid, errors, enrichedItems) = ValidateCartItems(cart.Items, productMap, reservationMap);
            if (!isValid)
            {
                return OperationResult<(Cart, Dictionary<string, Product>, List<CartItemViewModel>)>
                    .FailureResult(400, "Quantity validation failed: " + string.Join("; ", errors));
            }

            return OperationResult<(Cart, Dictionary<string, Product>, List<CartItemViewModel>)>
                .SuccessResult((cart, productMap, enrichedItems));
        }

        private async Task<OperationResult<List<ProductReservation>>> CreateTemporaryReservations(
            Cart cart,
            Guid userId,
            Dictionary<string, Product> productMap,
            IClientSessionHandle session)
        {
            var newReservations = cart.Items.Select(item => new ProductReservation
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                UserId = userId,
                ReservedQuantity = item.Quantity,
                IsConfirmed = false,
            }).ToList();

            var reserveResult = await _productReservationRepository.ReserveProductsAsync(newReservations, session);
            if (!reserveResult.Success || reserveResult.Data == null)
            {
                var error = reserveResult.Errors != null
                    ? string.Join(", ", reserveResult.Errors)
                    : "Failed to create reservations";
                return OperationResult<List<ProductReservation>>.FailureResult(reserveResult?.StatusCode ?? 500, error);
            }

            return OperationResult<List<ProductReservation>>.SuccessResult(newReservations);
        }

        private async Task<OperationResult<CheckoutSessionResponse>> FulfillOrder(
            Session stripeSession,
            Guid userId,
            Cart cart,
            List<ProductReservation> reservations,
            Dictionary<string, Product> productMap,
            List<CartItemViewModel> enrichedItems,
            IClientSessionHandle session)
        {
            try
            {
                // 1. Record payment
                var paymentResult = await RecordPayment(stripeSession, userId, cart.Items.Count, session);
                if (paymentResult==null || paymentResult.Data==null || !paymentResult.Success)
                {
                    await _unitOfWork.AbortAsync();
                    var error = paymentResult!.Errors != null
                  ? string.Join(", ", paymentResult.Errors)
                  : "Failed to retrieve reservations";
                    return OperationResult<CheckoutSessionResponse>.FailureResult(
                        paymentResult?.StatusCode ?? 500,error);
                }
                    

                // 2. Confirm reservations
                var confirmResult = await ConfirmReservations(reservations, session);
             
                if (confirmResult==null || confirmResult.Data==null || !confirmResult.Success)
                {
                    await _unitOfWork.AbortAsync();
                    var error = paymentResult.Errors != null
                 ? string.Join(", ", paymentResult.Errors)
                 : "Failed to confirm reservations";
                    return OperationResult<CheckoutSessionResponse>.FailureResult(
                       confirmResult?.StatusCode ?? 500,error);
                }

                // 3. Update product quantities
                var updateResult = await UpdateProductQuantities(reservations, productMap, session);
                if (updateResult == null || updateResult.Data == null || !updateResult.Success) 
                {
                    await _unitOfWork.AbortAsync();
                    var error = updateResult!.Errors != null
                        ? string.Join(", ", updateResult.Errors)
                        : "Failed to update product quantities";
                    return OperationResult<CheckoutSessionResponse>.FailureResult(
                       updateResult?.StatusCode ?? 500, error);
                   
                }
                

                // 4. Create order
                var orderResult = await CreateOrder(userId, enrichedItems, productMap, session);
                if (!orderResult.Success)
                {
                    await _unitOfWork.AbortAsync();
                    var error = updateResult.Errors != null
                       ? string.Join(", ", updateResult.Errors)
                       : "Failed to create order";
                    return OperationResult<CheckoutSessionResponse>.FailureResult(
                       orderResult?.StatusCode ?? 500,error);
                }
                  
                // 5. Clear cart
                cart.Items = new List<CartItem>();
                var cartResult = await _cartRepository.UpdateCartAsync(cart, session);
                if (cartResult == null || cartResult.Data == null || !cartResult.Success)
                {
                    await _unitOfWork.AbortAsync();
                    var error = cartResult!.Errors != null
                      ? string.Join(", ", cartResult.Errors)
                      : "Failed to clear cart";
                    return OperationResult<CheckoutSessionResponse>.FailureResult(
                        cartResult?.StatusCode ?? 500,error);
                }

                return OperationResult<CheckoutSessionResponse>.SuccessResult(
                    new CheckoutSessionResponse
                    {
                        SessionId = stripeSession.Id,
                        Url = stripeSession.Url,
                        ExpiresAt = stripeSession.ExpiresAt
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order fulfillment failed for user {UserId}", userId);
                return OperationResult<CheckoutSessionResponse>.FailureResult(500, "Order fulfillment failed");
            }
        }

        private async Task<OperationResult<Payment>> RecordPayment(
            Session stripeSession,
            Guid userId,
            int itemCount,
            IClientSessionHandle session)
        {
            var paymentRecord = new Payment
            {
                Id = Guid.NewGuid(),
                CustomerId = userId,
                StripeSessionId = stripeSession.Id,
                PaymentIntentId = stripeSession.PaymentIntentId,
                Amount = (decimal)stripeSession.AmountTotal! / 100m,
                Currency = stripeSession.Currency,
                Status = "pending", 
                CreatedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, string> { { "ItemsCount", itemCount.ToString() } }
            };

            return await _paymentRepository.CreatePaymentAsync(paymentRecord, session);
        }

        private async Task<OperationResult<List<ProductReservation>>> ConfirmReservations(
            List<ProductReservation> reservations,
            IClientSessionHandle session)
        {
            reservations.ForEach(r => r.IsConfirmed = true);
            return await _productReservationRepository.EditProductsAsync(reservations, session);
        }

        private async Task<OperationResult<List<Product>>> UpdateProductQuantities(
            List<ProductReservation> reservations,
            Dictionary<string, Product> productMap,
            IClientSessionHandle session)
        {
            var productsToUpdate = new List<Product>();
            var productItems = new List<ProductItem>();

            foreach (var reservation in reservations)
            {
                if (productMap.TryGetValue(reservation.ProductId.ToString(), out var product))
                {
                    var quantity = product.Quantity - reservation.ReservedQuantity;
                    product.Quantity = quantity;
                    productsToUpdate.Add(product);
                    var productItem = new ProductItem
                    {
                        Id = product.Id,
                        /*VendorId = product.VendorId,
                        Name = product.Name,
                        CategoryName = product.CategoryName,
                        Price = product.Price,*/
                        Quantity = quantity

                    };
                    productItems.Add(productItem);
                }
             
            }
        
            var resposne = await _elasticsearchService.BulkUpdateAsync(productItems);
            if (resposne == null || !resposne.Success || !resposne.Data)
            {
                var error = resposne!.Errors != null
                     ? string.Join(", ", resposne.Errors)
                     : "Failed to update";
                return OperationResult<List<Product>>.FailureResult(
                    resposne?.StatusCode ?? 500, error);
            }

            return await _productRepository.EditProductsAsync(productsToUpdate, session);
        }

        private async Task<OperationResult<Order>> CreateOrder(
            Guid userId,
            List<CartItemViewModel> enrichedItems,
            Dictionary<string, Product> productMap,
            IClientSessionHandle session)
        {
            
            var items = enrichedItems.Select(item =>
            {
                productMap.TryGetValue(item.ProductId.ToString(), out var product);
                return new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item!.ProductName!,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    VendorId = product!.VendorId
                };
            }).ToList();
            var totalCost = items.Select(i => i.Price*i.Quantity).Sum();
            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderDate = DateTime.Now,
                CustomerId = userId,
                Items = items,
                TotalCost = totalCost,
            };

            return await _orderRepository.CreateOrderAsync(order, session);
        }
        private (bool isValid, List<string> errors, List<CartItemViewModel> enrichedItems)
          ValidateCartItems(List<CartItem> items, Dictionary<string, Product> productMap,
                           Dictionary<Guid, int> reservationMap)
        {
            var errors = new List<string>();
            var enrichedItems = new List<CartItemViewModel>();

            foreach (var item in items)
            {
                if (!productMap.TryGetValue(item.ProductId.ToString(), out var product))
                {
                    errors.Add($"Product {item.ProductId} not found");
                    continue;
                }

                var available = product.Quantity - reservationMap.GetValueOrDefault(item.ProductId);
                if (item.Quantity > available)
                {
                    errors.Add($"{product.Name}: Available {available}, Requested {item.Quantity}");
                    continue;
                }

                enrichedItems.Add(new CartItemViewModel
                {
                    ProductId = item.ProductId,
                    ProductName = product.Name,
                    Price = (decimal)product.Price!,
                    Quantity = item.Quantity
                });
            }

            return (!errors.Any(), errors, enrichedItems);
        }

        public async Task<OperationResult<List<ProductItem>>> SearchAsync(string searchQuery)
        {
            try
            {
                var getProductsResult = await _elasticsearchService.SearchAsync(searchQuery);

                if (getProductsResult == null || !getProductsResult.Success || getProductsResult.Data == null)
                {
                    var error = getProductsResult?.Errors != null
                        ? string.Join(", ", getProductsResult.Errors)
                        : "Something went wrong";
                    return OperationResult<List<ProductItem>>.FailureResult(getProductsResult?.StatusCode ?? 500, error);
                }

                var productItems = getProductsResult.Data;
                return OperationResult<List<ProductItem>>.SuccessResult(productItems);
            }
            catch (Exception ex)
            {
                return OperationResult<List<ProductItem>>.FailureResult(500, ex.Message);
            }
        }

        public async Task<OperationResult<List<Order>>> GetOrders(string customerId)
        {
            try
            {
                if(customerId == null || !Guid.TryParse(customerId, out var userId))

                {
                    return OperationResult<List<Order>>.FailureResult(400,"Invaild customerId");
                }
                var getOrderResult = await _orderRepository.GetOrdersAsync(userId);
                if (getOrderResult == null || !getOrderResult.Success || getOrderResult.Data == null)
                {
                    var error = getOrderResult?.Errors != null
                        ? string.Join(", ", getOrderResult.Errors)
                        : "Something went wrong";
                    return OperationResult<List<Order>>.FailureResult(getOrderResult?.StatusCode ?? 500, error);
                }
                var orders = getOrderResult.Data;
                return OperationResult<List<Order>>.SuccessResult(orders);
            }
            catch(Exception ex)
            {
                return OperationResult<List<Order>>.FailureResult(500, ex.Message);
            }
           
        }
    }
}
