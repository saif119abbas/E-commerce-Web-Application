using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace E_Commerce.Models
{
    [CollectionName("Users")]
    public class User : MongoIdentityUser<Guid>
    {
    }
}
