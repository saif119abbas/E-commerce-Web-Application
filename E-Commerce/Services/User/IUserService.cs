using E_Commerce.Models;
using E_Commerce.Utilities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace E_Commerce.Services
{
    public interface IUserService
    {
        Task<OperationResult<UserAuth>> LoginAsync(LoginModel model);
        Task<OperationResult<UserAuth>> SignupAsync(UserModel model);
        Task<List<SelectListItem>> GetAllRolesAsSelectListAsync();
    }
}
