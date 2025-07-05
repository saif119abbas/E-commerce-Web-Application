using E_Commerce.Models;
using E_Commerce.Utilities;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Driver;

namespace E_Commerce.Repositories
{
    public interface IUserRepository
    {
        Task<OperationResult<UserAuth>> LoginAsync(LoginModel model);
        Task<OperationResult<UserAuth>> SignupAsync(UserModel model);
        Task<List<SelectListItem>> GetAllRolesAsSelectListAsync();
        Task DeleteUserAsync(Guid userId);
    }
}
