using E_Commerce.Utilities;
using E_Commerce.Models;
using MongoDB.Driver;
namespace E_Commerce.Repositories
{
    public interface IVendorRepository
    {
        Task<OperationResult<Vendor>> GetVendorByIdAsync(Guid vendorId);
        Task<OperationResult<Vendor>> CreateVendorAsync(Vendor model, IClientSessionHandle session);
  
    }
}
