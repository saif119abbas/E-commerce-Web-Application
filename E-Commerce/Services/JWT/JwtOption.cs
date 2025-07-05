namespace E_Commerce.Services
{
    public class JwtOptions
    {
        public string? Issure { get; init; }
        public string? Audience { get; init; }
        public string? Key { get; init; }
    }
}
