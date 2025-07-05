using E_Commerce.Models;
using E_Commerce.Utilities;
using System.Threading.Tasks;

namespace E_Commerce.Services
{
    public interface ICategoryService
    {
        Task<OperationResult<List<ProductDTO>>> GetAllCategories();
    }
}
