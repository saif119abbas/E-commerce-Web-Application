using E_Commerce.Models;
using E_Commerce.Repositories;
using E_Commerce.Utilities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace E_Commerce.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IVendorRepository _vendorRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ICartRepository _cartRepository;
        private IUnitOfWork _unitOfWork;
        private readonly IJwtProvider _jwtProvider;


        public UserService(
            IUserRepository userRepository,
            IVendorRepository vendorRepository,
            ICustomerRepository customerRepository,
            ICartRepository cartRepository,
            IUnitOfWork unitOfWork,
            IJwtProvider jwtProvider)
        {
            _userRepository = userRepository;
            _vendorRepository = vendorRepository;
            _customerRepository = customerRepository;
            _cartRepository = cartRepository;
            _unitOfWork = unitOfWork;
            _jwtProvider = jwtProvider;

        }

        public async Task<List<SelectListItem>> GetAllRolesAsSelectListAsync() => await 
            _userRepository.GetAllRolesAsSelectListAsync();

        public async Task<OperationResult<UserAuth>> LoginAsync(LoginModel model) =>
            await _userRepository.LoginAsync(model);
        public async Task<OperationResult<UserAuth>> SignupAsync(UserModel model)
        {
            if (model == null)
                return OperationResult<UserAuth>.FailureResult(400, "Invalid Parameters");

            if (model.Role != Roles.Vendor && model.Role != Roles.Customer)
            {
                return OperationResult<UserAuth>.FailureResult(400, "Invalid role specified");
            }

            // Step 1: First handle the identity portion (non-transactional)
            var resultAddUser = await _userRepository.SignupAsync(model);

            if (resultAddUser == null || !resultAddUser.Success || resultAddUser.Data == null)
            {
                var error = resultAddUser?.Errors != null
                    ? string.Join(", ", resultAddUser.Errors)
                    : "Something went wrong";
                return OperationResult<UserAuth>.FailureResult(resultAddUser?.StatusCode ?? 500, error);
            }
         

            try
            {
                await _unitOfWork.StartTransactionAsync();
                var session = _unitOfWork.Session;

                if (resultAddUser.Data.Role == Roles.Vendor)
                {
                    var vendor = new Vendor
                    {
                        UserId = resultAddUser.Data.Id!.Value,
                        UserName = resultAddUser.Data.UserName,
                        Email = resultAddUser.Data.Email
                    };

                    var addVendorResult = await _vendorRepository.CreateVendorAsync(vendor, session);
                    if (addVendorResult == null || !addVendorResult.Success || addVendorResult.Data == null)
                    {
                        await _userRepository.DeleteUserAsync(resultAddUser.Data.Id!.Value);
                        var error = addVendorResult?.Errors != null
                            ? string.Join(", ", addVendorResult.Errors)
                            : "Something went wrong";
                        return OperationResult<UserAuth>.FailureResult(addVendorResult?.StatusCode ?? 500, error);
                    }
                   
                }
                else if (resultAddUser.Data.Role == Roles.Customer)
                {
                    var customer = new Customer
                    {
                        UserId = resultAddUser.Data.Id!.Value,
                        UserName = resultAddUser.Data.UserName,
                        Email = resultAddUser.Data.Email
                    };

                    var addCustomerResult = await _customerRepository.CreateCustomerAsync(customer, session);
                    if (addCustomerResult == null || !addCustomerResult.Success || addCustomerResult.Data == null)
                    {
                        await _userRepository.DeleteUserAsync(resultAddUser.Data.Id!.Value);
                        var error = addCustomerResult?.Errors != null
                            ? string.Join(", ", addCustomerResult.Errors)
                            : "Something went wrong";
                        return OperationResult<UserAuth>.FailureResult(addCustomerResult?.StatusCode ?? 500, error);
                    }
                   

                    var cart = new Cart
                    {
                        Id = Guid.NewGuid(),
                        UserId = resultAddUser.Data.Id.Value,
                        Items = new List<CartItem>()
                    };

                    var addCartResult = await _cartRepository.AddCartAsync(cart, session);
                    if (addCartResult == null || !addCartResult.Success || addCartResult.Data == null)
                    {
                        await _userRepository.DeleteUserAsync(resultAddUser.Data.Id!.Value);
                        var error = addCartResult?.Errors != null
                            ? string.Join(", ", addCartResult.Errors)
                            : "Something went wrong";
                        return OperationResult<UserAuth>.FailureResult(addCartResult?.StatusCode ?? 500, error);
                    }

                 
                }

                await _unitOfWork.CommitAsync();
                return OperationResult<UserAuth>.SuccessResult(resultAddUser.Data);
            }
            catch (Exception ex)
            {
                await _unitOfWork.AbortAsync();
                await _userRepository.DeleteUserAsync(resultAddUser.Data.Id!.Value);
                return OperationResult<UserAuth>.FailureResult(500, ex.Message);
            }
        }
    }
}
