using E_Commerce.Models;
using E_Commerce.Utilities;

namespace E_Commerce.Services
{
    public class CategoryService : ICategoryService
    {
        Task<OperationResult<List<ProductDTO>>> ICategoryService.GetAllCategories()
        {
            throw new NotImplementedException();
        }
    }
}
