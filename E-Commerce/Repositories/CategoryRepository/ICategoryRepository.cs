using E_Commerce.Models;
using E_Commerce.Utilities;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Driver;
namespace E_Commerce.Repositories
{
    public interface ICategoryRepository
    {
        Task<OperationResult<Category>> AddCategoryAsync(Category category, IClientSessionHandle session = null);
        Task<OperationResult<Category>> GetCategoryByIdAsync(string id );
        Task<OperationResult<Category>> GetCategoryByNameAsync(string name );
        Task<OperationResult<List<Category>>> GetAllCategoriesAsync();

    }
}
