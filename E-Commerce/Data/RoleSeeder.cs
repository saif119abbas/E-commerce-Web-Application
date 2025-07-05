using E_Commerce.Models;
using Microsoft.AspNetCore.Identity;

public static class RoleSeeder
{
    public async static Task SeedRolesAsync(IConfiguration configuration, RoleManager<UserRoles> roleManager)
    {
        foreach (var role in new[] { Roles.Vendor, Roles.Customer })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new UserRoles(role));
            }
        }
    }
}
