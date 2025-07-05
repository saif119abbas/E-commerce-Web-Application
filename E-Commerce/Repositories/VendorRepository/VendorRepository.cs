using E_Commerce.Models;
using E_Commerce.Utilities;
using Microsoft.CodeAnalysis;
using MongoDB.Driver;


namespace E_Commerce.Repositories
{
    public class VendorRepository : MongoRepository<Vendor>,IVendorRepository
    {
        public VendorRepository(IMongoDatabase database, IUnitOfWork unitOfWork)
         : base(database, unitOfWork, "Vendors")
        {
          
        }
        public async Task<OperationResult<Vendor>> GetVendorByIdAsync(Guid vendorId)
        {
            try
            {
                var vendor = await findVendor(vendorId);
                if (vendor == null)
                    return OperationResult<Vendor>.FailureResult(404, "The user doesn't exsit");
                var result = new VendorDTO
                {
                    Id = vendorId,
                    UserName = vendor.UserName,
               
                };
                return OperationResult<Vendor>.SuccessResult(vendor);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case MongoWriteException mwe when mwe.WriteError.Category == ServerErrorCategory.DuplicateKey:
                    case MongoCommandException mce when mce.Code == 11000:
                        return OperationResult<Vendor>.FailureResult(409, "Duplicate key violation");

                    case TimeoutException:
                        return OperationResult<Vendor>.FailureResult(504, "Database operation timed out");

                    case MongoException me:
                        return OperationResult<Vendor>.FailureResult(500, $"Database error: {me.Message}");

                    default:
                        return OperationResult<Vendor>.FailureResult(500, $"Unexpected error: {ex.Message}");
                }
            }
        }
   
        public async Task<OperationResult<Vendor>> CreateVendorAsync(Vendor model, IClientSessionHandle session=null)
        {
            try
            {
                if (session == null)
                {
                    await _collection.InsertOneAsync(model);
                }
                else
                {
                    await _collection.InsertOneAsync(session,model);
                }

                
                return OperationResult<Vendor>.SuccessResult(model);
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case MongoWriteException mwe when mwe.WriteError.Category == ServerErrorCategory.DuplicateKey:
                    case MongoCommandException mce when mce.Code == 11000:
                        return OperationResult<Vendor>.FailureResult(409, "Duplicate key violation");

                    case TimeoutException:
                        return OperationResult<Vendor>.FailureResult(504, "Database operation timed out");

                    case MongoException me:
                        return OperationResult<Vendor>.FailureResult(500, $"Database error: {me.Message}");

                    default:
                        return OperationResult<Vendor>.FailureResult(500, $"Unexpected error: {ex.Message}");
                }
            }
        }
        private async Task<Vendor> findVendor(Guid vendorId) =>
            await _collection.Find(v => v.UserId == vendorId).FirstOrDefaultAsync();
        public async Task<bool> UpdateVendorAsync( Vendor vendor)
        {
            var filter = Builders<Vendor>.Filter.Eq(v => v.Id, vendor.Id);
            var updateResult = await _collection.ReplaceOneAsync( filter, vendor);
            return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
        }

    }
    
}
