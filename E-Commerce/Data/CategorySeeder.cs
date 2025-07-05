using E_Commerce.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace E_Commerce.Data
{
    public static class CategorySeeder
    {
        public static void SeedCategories(IConfiguration configuration)
        {
            var connectionString = configuration["DatabaseSetup:ConnectionString"];
            var dbName = configuration["DatabaseSetup:DatabaseName"];

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(dbName);
            var categories = database.GetCollection<Category>("Categories");

            var existing = categories.Find(_ => true).Any();
            if (existing) return;

            var predefinedCategories = new List<Category>
            {
                new Category { Id = Guid.NewGuid(), Name = "Electronics" },
                new Category { Id = Guid.NewGuid(), Name = "Books" },
                new Category { Id = Guid.NewGuid(), Name = "Clothing" },
                new Category { Id = Guid.NewGuid(), Name = "Home & Kitchen" },
                new Category { Id = Guid.NewGuid(), Name = "Toys" },
                new Category { Id = Guid.NewGuid(), Name = "Other" },
            };

            categories.InsertMany(predefinedCategories);
        }
    }
}
