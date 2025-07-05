using E_Commerce.Models;
namespace E_Commerce.Services
{
    public interface IJwtProvider
    {
        public string Generate(User user, IList<string> roles);
        public string GetUserIdFromToken(string token);
    }
}

