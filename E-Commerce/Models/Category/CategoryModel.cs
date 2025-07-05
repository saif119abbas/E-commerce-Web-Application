using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;


namespace E_Commerce.Models
{
    public class CategoryModel
    {
            [Required(ErrorMessage = "The name of category is required")]
            [BsonElement("name")]
            public string? Name { get; set; }
  
    }
}
