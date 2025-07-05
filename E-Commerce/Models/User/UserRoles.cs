using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace E_Commerce.Models
{
    [CollectionName("Roles")]
    public  class UserRoles: MongoIdentityRole<Guid>
    {
        public UserRoles() : base() { }
        public UserRoles(string roleName) : base(roleName) { }

    }
}
