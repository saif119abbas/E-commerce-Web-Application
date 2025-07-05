using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Models
{
    public class UserAuth
    {
        public Guid? Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string ? Role {  get; set; }
        public string? token { get; set; }=new string("");

    }
}
